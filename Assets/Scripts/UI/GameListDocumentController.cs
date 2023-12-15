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
using System.IO;
using System.Linq;
using Nofun.Data;
using Nofun.Data.Model;
using Nofun.Driver.UI;
using Nofun.Parser;
using Nofun.Plugins;
using Nofun.Services;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Nofun.UI
{
    public class GameListDocumentController : MonoBehaviour
    {
        private static readonly string GameDatabaseFileName = "games.db";
        private string GameDatabasePath => $"{Application.streamingAssetsPath}/{GameDatabaseFileName}";

        private UIDocument _document;
        private Button _installButton;
        private VisualElement _gameList;
        private GameDatabase _gameDatabase;

        [Header("UI")]
        [SerializeField] private VisualTreeAsset gameEntryTemplate;
        [SerializeField] private GameIconManifest gameIconManifest;

        [Header("Runner")]
        [SerializeField] private NofunRunner runner;

        private ITranslationService translationService;
        private IDialogService dialogService;

        private string GamePathRoot => $"{Application.persistentDataPath}/__Games";

        private string GetGamePath(string gameFileName)
        {
            return $"{GamePathRoot}/{gameFileName}";
        }

        private string GetGamePath(GameInfo gameInfo) => GetGamePath(gameInfo.GameFileName);

        private void Awake()
        {
            translationService = EmulatorLifetimeScope.ContainerInstance.Resolve<ITranslationService>();
            dialogService = EmulatorLifetimeScope.ContainerInstance.Resolve<IDialogService>();

            _document = GetComponent<UIDocument>();
            _installButton = _document.rootVisualElement.Q<Button>("InstallButton");
            _gameList = _document.rootVisualElement.Q<VisualElement>("GameList");
            _gameDatabase = new GameDatabase(GameDatabasePath);

            Directory.CreateDirectory(GamePathRoot);

            _installButton.clicked += OnInstallButtonClicked;

            LoadGameList();
        }

        private void OnDestroy()
        {
            _installButton.clicked -= OnInstallButtonClicked;
        }

        private void OnGameIconClicked(string gameFileName)
        {
            if (runner != null)
            {
                if (!runner.isActiveAndEnabled)
                {
                    string gamePath = GetGamePath(gameFileName);
                    if (!File.Exists(gamePath))
                    {
                        dialogService.Show(IUIDriver.Severity.Error,
                            IUIDriver.ButtonType.OK,
                            translationService.Translate("Error"),
                            translationService.Translate("Error_Description_NoGameFileFound"),
                            null);

                        return;
                    }

                    runner.gameObject.SetActive(true);
                    runner.Launch(gamePath);

                    gameObject.SetActive(false);
                }
            }
        }

        private void LoadGameList()
        {
            foreach (var child in _gameList.Children())
            {
                if (child.userData is GameInfoEntryController controller)
                {
                    controller.OnGameInfoChoosen -= OnGameIconClicked;
                }
            }

            _gameList.Clear();

            var gameInfos = _gameDatabase.AllGames;

            foreach (var gameInfo in gameInfos)
            {
                var gameInfoEntry = gameEntryTemplate.Instantiate();
                var gameInfoEntryBinder = new GameInfoEntryController(gameIconManifest);

                gameInfoEntryBinder.SetVisualElement(gameInfoEntry);
                gameInfoEntryBinder.BindData(gameInfo);
                gameInfoEntryBinder.OnGameInfoChoosen += OnGameIconClicked;

                _gameList.Add(gameInfoEntry);
            }
        }

        private void InstallGame(string path)
        {
            using (var executableFile = File.OpenRead(path))
            {
                try
                {
                    VMGPExecutable executable = new VMGPExecutable(executableFile);

                    VMMetaInfoReader metaInfoReader = executable.GetMetaInfo();
                    if (metaInfoReader == null)
                    {
                        dialogService.Show(IUIDriver.Severity.Error,
                            IUIDriver.ButtonType.OK,
                            translationService.Translate("Error"),
                            translationService.Translate("Error_Description_NoGameInfo"),
                            null);

                        return;
                    }

                    string titleName = metaInfoReader.Get("Title");
                    string vendor = metaInfoReader.Get("Vendor");
                    string version = metaInfoReader.Get("Program version");

                    Debug.Log($"Title: {titleName}, Vendor: {vendor}, Version: {version}");

                    if (titleName == null)
                    {
                        dialogService.Show(IUIDriver.Severity.Error,
                            IUIDriver.ButtonType.OK,
                            translationService.Translate("Error"),
                            translationService.Translate("Error_Description_NoGameTitle"),
                            null);

                        return;
                    }

                    var versionNumbers = (version == null) ? null : version.Split(".", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    GameInfo gameInfo = new GameInfo(titleName, vendor ?? null,
                        versionNumbers != null && versionNumbers.Length >= 1 ? versionNumbers[0] : 0,
                        versionNumbers != null && versionNumbers.Length >= 2 ? versionNumbers[1] : 0,
                        versionNumbers != null && versionNumbers.Length >= 3 ? versionNumbers[2] : 0);

                    if (!_gameDatabase.AddGame(gameInfo))
                    {
                        dialogService.Show(IUIDriver.Severity.Error,
                            IUIDriver.ButtonType.OK,
                            translationService.Translate("Error"),
                            translationService.Translate("Error_Description_GameAlreadyInstalled"),
                            null);

                        return;
                    }
                    else
                    {
                        // Save the game into the persistent data folder
                        string gamePath = GetGamePath(gameInfo);
                        using (FileStream storedFile = File.OpenWrite(gamePath))
                        {
                            executableFile.CopyTo(storedFile);
                        }

                        dialogService.Show(IUIDriver.Severity.Info,
                            IUIDriver.ButtonType.OK,
                            translationService.Translate("Success"),
                            translationService.Translate("Success_Description_Install"),
                            null);

                        LoadGameList();
                    }
                }
                catch
                {
                    dialogService.Show(IUIDriver.Severity.Error,
                        IUIDriver.ButtonType.OK,
                        translationService.Translate("Error"),
                        translationService.Translate("Error_Description_NotMophun"),
                        null);
                }
            }
        }

        private void OnInstallButtonClicked()
        {
            bool permissionGranted = FilePicker.OpenPickFileDialog(new FilterItem[]
            {
                new FilterItem
                {
                    name = "Mophun game",
                    spec = "mpn"
                }
            }, (string path) =>
            {
                if (path != null)
                {
                    InstallGame(path);
                }
            });

            if (!permissionGranted)
            {
                Debug.Log("Todo: Show error message not granted");
            }
        }
    }
}
