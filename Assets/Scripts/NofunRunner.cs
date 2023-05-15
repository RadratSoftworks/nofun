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

using Nofun.Driver.Unity.Audio;
using Nofun.Driver.Unity.Graphics;
using Nofun.Driver.Unity.Input;
using Nofun.Driver.Unity.Time;
using Nofun.Parser;
using Nofun.Util.Unity;
using Nofun.VM;
using System.IO;
using UnityEngine;

using System.Threading;

namespace Nofun
{
    public class NofunRunner: MonoBehaviour
    {
        [SerializeField]
        private InputDriver inputDriver;

        [SerializeField]
        private AudioDriver audioDriver;

        [SerializeField]
        private GraphicDriver graphicDriver;

        [SerializeField]
        private GameObject buttonControl;

        [SerializeField]
        private GameObject failedLaunchDialog;

        private TimeDriver timeDriver;

        [Range(1, 60)]
        [SerializeField]
        private int fpsLimit = 30;

        private VMGPExecutable executable;
        private VMSystem system;
        private Thread systemThread;
        private bool started = false;
        private bool failed = false;

        [SerializeField]
        private string executableFilePath = "E:\\spacebox.mpn";

        private void SetupLogger()
        {
            Util.Logging.Logger.AddTarget(new UnityLogTarget());
        }

        private void OnDestroy()
        {
            system.Stop();
        }

        private void Start()
        {
            SetupLogger();

            Stream gameStream = null;
            string targetExecutable = executableFilePath;

#if !UNITY_EDITOR && NOFUN_PRODUCTION
#if UNITY_ANDROID
            try
            {
                gameStream = new MophunAndroidFileStream();
            }
            catch (System.Exception ex)
            {
                failedLaunchDialog.SetActive(true);
                failed = true;

                return;
            }
#else
            string[] cmdLines = System.Environment.GetCommandLineArgs();

            if (cmdLines.Length >= 2)
            {
                targetExecutable = cmdLines[1];
            }
#endif
#endif

#if UNITY_EDITOR
            gameStream = new FileStream(targetExecutable, FileMode.Open, FileAccess.ReadWrite,
                FileShare.Read);
#endif

            buttonControl.SetActive(Application.isMobilePlatform);

            timeDriver = new TimeDriver();

            executable = new VMGPExecutable(gameStream);

            system = new VMSystem(executable, new VMSystemCreateParameters(graphicDriver, inputDriver, audioDriver, timeDriver,
                Application.persistentDataPath, targetExecutable));

            // Setup graphics driver
            graphicDriver.StopProcessorAction = () => system.Processor.Stop();

            systemThread = new Thread(new ThreadStart(() =>
            {
                while (!system.ShouldStop)
                {
                    system.Run();
                }
            }));
        }

        private void Update()
        {
            if (failed)
            {
                return;
            }

            graphicDriver.FpsLimit = fpsLimit;
            
            if (!started)
            {
                systemThread.Start();
                started = true;
            }

            timeDriver.Update();
        }
    }
}