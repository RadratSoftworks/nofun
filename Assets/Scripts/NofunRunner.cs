/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Nofun.Driver.Unity.Audio;
using Nofun.Driver.Unity.Graphics;
using Nofun.Driver.Unity.Input;
using Nofun.Driver.Unity.Time;
using Nofun.Driver.Unity.UI;
using Nofun.Parser;
using Nofun.Util.Unity;
using Nofun.VM;
using System.IO;
using UnityEngine;

using System.Threading;
using Nofun.UI;
using Nofun.Settings;
using System.Collections;
using Nofun.Services;
using VContainer;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Nofun
{
    public class NofunRunner : MonoBehaviour
    {
        [Header("Drivers")]
        [SerializeField] private InputDriver inputDriver;
        [SerializeField] private AudioDriver audioDriver;
        [SerializeField] private GraphicDriver graphicDriver;
        [SerializeField] private UIDriver uiDriver;
        private TimeDriver timeDriver;

        [Header("UI")]
        [SerializeField] private SettingDocumentController settingDocument;
        [SerializeField] private GameDetailsDocumentController gameDetailsDocument;
        [SerializeField] private GameListDocumentController gameListDocumentController;
        [SerializeField] private float waitTimeBeforeNotifyUserOfLLVM = 0.2f;

        [Header("Settings")]
        [Range(1, 60)][SerializeField] private int fpsLimit = 30;
        [SerializeField] private string executableFilePath = "E:\\spacebox.mpn";
        [SerializeField] private bool immediatelyRun = false;
        [SerializeField] private bool enableLLVM = false;

        private VMGPExecutable executable;
        private VMSystem system;
        private GameSettingsManager settingManager;
        private Thread systemThread;
        private bool started = false;
        private bool failed = false;
        private bool settingActive = false;
        private bool launchRequested = false;

        private bool llvmPrepared = false;
        private int llvmPreparingDialogId = -1;

        [Inject] private ScreenManager screenManager;
        [Inject] private IDialogService dialogService;
        [Inject] private ITranslationService translationService;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
        [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
        public static extern System.IntPtr GetActiveWindow();

        private System.IntPtr currentWindow;
#endif

        [Inject]
        public void Construct(ScreenManager injectScreenManager)
        {
            screenManager = injectScreenManager;
        }

        private void SetupLogger()
        {
            Util.Logging.Logger.AddTarget(new UnityLogTarget());
        }

        private void OnDestroy()
        {
            settingDocument.Finished -= FinishSettingDocument;
            settingDocument.ExitGameRequested -= HandleExitGame;

            if (system != null)
            {
                system.Stop();
            }
        }

        private void FinishSettingDocument(bool isCancel)
        {
            if (!isCancel)
            {
                GameSetting? setting = settingManager.Get(system.GameName);
                if (setting != null)
                {
                    graphicDriver.FpsLimit = setting.Value.fps;

                    if (setting.Value.screenMode != ScreenMode.Fullscreen)
                    {
                        screenManager.ScreenOrientation = setting.Value.orientation;
                    }
                }
            }
            else
            {
                if (!started)
                {
                    // Cancel launch
                    launchRequested = false;
                    gameListDocumentController.ImmediateShow();
                }
            }

            settingActive = false;
            JobScheduler.Paused = false;
        }

        private IEnumerator ShowGameListDelay()
        {
            yield return new WaitForSeconds(0.2f);
            gameListDocumentController.ImmediateShow();
        }

        private void HandleExitGame()
        {
            settingActive = false;
            JobScheduler.Paused = false;

            system.Stop();
            StartCoroutine(ShowGameListDelay());
        }

        private void OpenGameSetting()
        {
            settingDocument.Show(showExitGameButton: started);

            settingActive = true;
            JobScheduler.Paused = true;
        }

        public void OnGameScreenCogButtonPressed()
        {
            OpenGameSetting();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            SetupLogger();

            settingManager = new(Application.persistentDataPath);
            timeDriver = new TimeDriver();

            gameDetailsDocument.Setup(settingManager);
        }

        private void Start()
        {
            Stream gameStream = null;

#if UNITY_EDITOR
            string targetExecutable = executableFilePath;
#else
            string targetExecutable = null;
#endif

#if !UNITY_EDITOR && NOFUN_PRODUCTION
#if UNITY_ANDROID
            try
            {
                gameStream = new MophunAndroidFileStream();
            }
            catch (System.Exception _)
            {
                return;
            }
#else
            string[] cmdLines = System.Environment.GetCommandLineArgs();

            if (cmdLines.Length >= 2)
            {
                targetExecutable = cmdLines[1];
            }
            else
            {
                return;
            }
#endif
#endif

#if UNITY_EDITOR || !UNITY_ANDROID
#if UNITY_EDITOR
            if (immediatelyRun)
            {
#endif
                gameStream = new FileStream(targetExecutable, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.Read);

                gameListDocumentController.ImmediateHide();
                launchRequested = true;

                StartGameImpl(gameStream, targetExecutable);
#if UNITY_EDITOR
            }
#endif
#endif
        }

        public void Launch(string gamePath)
        {
            Reset();

            executableFilePath = gamePath;
            launchRequested = true;

            FileStream stream = new FileStream(gamePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            StartGameImpl(stream, gamePath);
        }

        private void Reset()
        {
            bool shouldGc = false;

            if (system != null)
            {
                shouldGc = true;

                system.Dispose();
                system = null;
            }

            started = false;
            failed = false;
            launchRequested = false;

            if (executable != null)
            {
                executable.Dispose();
                executable = null;
            }

            if (shouldGc)
            {
                GC.Collect();
            }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowText(currentWindow, $"nofun");
#endif
        }

        public void StartGameImpl(Stream gameStream, string targetExecutable)
        {
            try
            {
                executable = new VMGPExecutable(gameStream);
                system = new VMSystem(executable, new VMSystemCreateParameters(graphicDriver, inputDriver, audioDriver, timeDriver, uiDriver,
                    Application.persistentDataPath, targetExecutable, enableLLVM));
            }
            catch (System.Exception _)
            {
                dialogService.Show(Severity.Info, ButtonType.OK,
                    null,
                    translationService.Translate("Error_Description_GameNotCompatible"),
                    value => Application.Quit());

                failed = true;

                return;
            }

            settingDocument.Setup(settingManager, system.GameName);

            settingDocument.Finished += FinishSettingDocument;
            settingDocument.ExitGameRequested += HandleExitGame;

            if (settingManager.Get(system.GameName) == null)
            {
                OpenGameSetting();
            }

            systemThread = new Thread(new ThreadStart(() =>
            {
                system.PostInitialize();
                llvmPrepared = true;

                while (!system.ShouldStop)
                {
                    system.Run();
                }

                Reset();
            }));

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            currentWindow = GetActiveWindow();
#endif
        }

        private IEnumerator InitializeGameRun()
        {
            GameSetting? setting = settingManager.Get(system.GameName);
            setting = setting ?? new GameSetting()
            {
                screenSizeX = 101,
                screenSizeY = 80,
                fps = 30,
                screenMode = ScreenMode.CustomSize,
                deviceModel = Module.VMGPCaps.SystemDeviceModel.SonyEricssonT300,
                systemVersion = SystemVersion.Version150,
                enableSoftwareScissor = false
            };

            system.GameSetting = setting.Value;

            // Change orientation first
            screenManager.ScreenOrientation = setting.Value.orientation;

            graphicDriver.Initialize((setting.Value.screenMode == ScreenMode.CustomSize) ?
                new Vector2(setting.Value.screenSizeX, setting.Value.screenSizeY) :
                Vector2.zero, setting.Value.enableSoftwareScissor);

            graphicDriver.FpsLimit = Mathf.Clamp(setting.Value.fps, 1, 120);
            systemThread.Start();

            if (setting.Value.cpuBackend == CPUBackend.LLVM)
            {
                llvmPrepared = false;
                llvmPreparingDialogId = -1;

                float elapsedTime = 0.0f;

                while (!llvmPrepared)
                {
                    if (waitTimeBeforeNotifyUserOfLLVM <= elapsedTime && llvmPreparingDialogId < 0)
                    {
                        llvmPreparingDialogId = dialogService.OpenBlocked(Severity.Info,
                            translationService.Translate("Info_Title_PreparingLLVM"),
                            translationService.Translate("Info_Description_PreparingLLVM"));
                    }

                    yield return null;

                    elapsedTime += Time.deltaTime;
                }

                if (llvmPreparingDialogId >= 0)
                {
                    dialogService.CloseBlocked(llvmPreparingDialogId);
                }
            }

            yield break;
        }

        private void Update()
        {
            if (settingActive || failed || !launchRequested)
            {
                return;
            }

            if (!started)
            {
                StartCoroutine(InitializeGameRun());
                started = true;
            }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (currentWindow != System.IntPtr.Zero)
            {
                SetWindowText(currentWindow, $"nofun - {system.GameName} - {graphicDriver.Fps} FPS");
            }
#endif

            timeDriver.Update();
        }
    }
}
