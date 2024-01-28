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
using Nofun.DynamicIcons;
using Nofun.Parser;
using Nofun.Services;
using Nofun.Plugins;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Nofun.UI
{
    public class GameListDocumentController : FlexibleUIDocumentController, IGameProvider
    {
        private static readonly string GameDatabaseFileName = "games.db";
        private string GameDatabasePath => $"{Application.persistentDataPath}/{GameDatabaseFileName}";

        private Button installButton;
        private VisualElement gameList;
        private GameDatabase gameDatabase;
        private TextField searchBar;

        [Header("UI")]
        [SerializeField] private VisualTreeAsset gameEntryTemplate;
        [SerializeField] private GameIconManifest gameIconManifest;
        [SerializeField] private Transform dynamicIconRendererContainer;
        [SerializeField] private GameDetailsDocumentController gameDetailsDocumentController;

        [Header("Runner")]
        [SerializeField] private NofunRunner runner;

        [Inject] private ITranslationService translationService;
        [Inject] private IDialogService dialogService;
        [Inject] private ILayoutService layoutService;
        private DynamicIconsProvider dynamicIconsProvider;

        private string GamePathRoot => $"{Application.persistentDataPath}/__Games";

        public string GetGamePath(string gameFileName)
        {
            return $"{GamePathRoot}/{gameFileName}";
        }

        private string GetGamePath(GameInfo gameInfo) => GetGamePath(gameInfo.GameFileName);

        public override void Awake()
        {
            base.Awake();

            if (!File.Exists(GameDatabasePath))
            {
                TextAsset asset = Resources.Load(GameDatabaseFileName) as TextAsset;

                if (asset != null)
                {
                    File.WriteAllBytes(GameDatabasePath, asset.bytes);
                }
            }

            gameDatabase = new GameDatabase(GameDatabasePath);
            dynamicIconsProvider = new DynamicIconsProvider(dynamicIconRendererContainer);

            Directory.CreateDirectory(GamePathRoot);
        }

        private void OnEnable()
        {
            layoutService.SetVisibility(false);

            installButton = document.rootVisualElement.Q<Button>("InstallButton");
            gameList = document.rootVisualElement.Q<VisualElement>("GameList");
            searchBar = document.rootVisualElement.Q<TextField>("SearchBar");
            installButton.clicked += OnInstallButtonClicked;
            gameDetailsDocumentController.OnGameInfoChoosen += OnGameIconClicked;
            gameDetailsDocumentController.OnGameRemovalRequested += RemoveGame;

            searchBar.RegisterValueChangedCallback(OnSearchBarContentChanged);

            gameList.RegisterCallback<GeometryChangedEvent>((_) =>
            {
                LoadGameList();
            });
        }

        private void OnDisable()
        {
            layoutService.SetVisibility(true);
            dynamicIconsProvider.Cleanup();

            installButton.clicked -= OnInstallButtonClicked;
            gameDetailsDocumentController.OnGameInfoChoosen -= OnGameIconClicked;
            gameDetailsDocumentController.OnGameRemovalRequested -= RemoveGame;
        }

        private void OnSearchBarContentChanged(ChangeEvent<string> newValue)
        {
            LoadGameList(newValue.newValue);
        }

        private void OnGameIconClicked(string gameFileName)
        {
            if (runner != null)
            {
                string gamePath = GetGamePath(gameFileName);
                if (!File.Exists(gamePath))
                {
                    dialogService.Show(Severity.Error,
                        ButtonType.OK,
                        translationService.Translate("Error"),
                        translationService.Translate("Error_Description_NoGameFileFound"),
                        null);

                    return;
                }

                runner.gameObject.SetActive(true);
                runner.Launch(gamePath);

                ImmediateHide();
            }
        }

        private void LoadGameList(string filter = "")
        {
            var gameInfos = string.IsNullOrEmpty(filter) ? gameDatabase.AllGames : gameDatabase.GamesByKeyword(filter);
            RebuildGameList(gameInfos);
        }

        private void RebuildGameList(GameInfo[] gameInfos)
        {
            foreach (var child in gameList.Children())
            {
                if (child.userData is GameInfoEntryController controller)
                {
                    controller.OnGameInfoChoosen -= OnGameIconClicked;
                }
            }

            gameList.Clear();

            foreach (var gameInfo in gameInfos)
            {
                var gameInfoEntry = gameEntryTemplate.Instantiate();
                var gameInfoEntryBinder = new GameInfoEntryController(gameIconManifest, dynamicIconsProvider, gameDetailsDocumentController);

                gameInfoEntryBinder.SetVisualElement(gameInfoEntry);
                gameInfoEntryBinder.BindData(gameInfo);
                gameInfoEntryBinder.OnGameInfoChoosen += OnGameIconClicked;

                gameList.Add(gameInfoEntry);
            }
        }

        private void RemoveGame(GameInfo gameInfo)
        {
            string gamePath = GetGamePath(gameInfo);
            if (File.Exists(gamePath))
            {
                File.Delete(gamePath);
            }

            gameDatabase.RemoveGame(gameInfo);
            LoadGameList();
        }

        private void InstallGame(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            using (var executableFile = File.OpenRead(path))
            {
                try
                {
                    VMGPExecutable executable = new VMGPExecutable(executableFile);

                    VMMetaInfoReader metaInfoReader = executable.GetMetaInfo();
                    if (metaInfoReader == null)
                    {
                        dialogService.Show(Severity.Error,
                            ButtonType.OK,
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
                        dialogService.Show(Severity.Error,
                            ButtonType.OK,
                            translationService.Translate("Error"),
                            translationService.Translate("Error_Description_NoGameTitle"),
                            null);

                        return;
                    }

                    int[] versionNumbers;

                    try
                    {
                        versionNumbers = (version == null)
                            ? null
                            : version.Split(".", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    }
                    catch
                    {
                        versionNumbers = new[] { 0, 0, 0 };
                    }

                    GameInfo gameInfo = new GameInfo(titleName, vendor ?? null,
                        versionNumbers != null && versionNumbers.Length >= 1 ? versionNumbers[0] : 0,
                        versionNumbers != null && versionNumbers.Length >= 2 ? versionNumbers[1] : 0,
                        versionNumbers != null && versionNumbers.Length >= 3 ? versionNumbers[2] : 0);

                    if (!gameDatabase.AddGame(gameInfo))
                    {
                        dialogService.Show(Severity.Error,
                            ButtonType.OK,
                            translationService.Translate("Error"),
                            translationService.Translate("Error_Description_GameAlreadyInstalled"),
                            null);

                        return;
                    }
                    else
                    {
                        // Save the game into the persistent data folder
                        string gamePath = GetGamePath(gameInfo);
                        File.Copy(path, gamePath, true);

                        dialogService.Show(Severity.Info,
                            ButtonType.OK,
                            translationService.Translate("Success"),
                            translationService.Translate("Success_Description_Install"),
                            null);

                        LoadGameList();
                    }
                }
                catch (Exception ex)
                {
                    dialogService.Show(Severity.Error,
                        ButtonType.OK,
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
                #if UNITY_EDITOR || !UNITY_ANDROID
                new FilterItem
                {
                    name = "Mophun game",
                    spec = "mpn"
                }
                #else
                new FilterItem
                {
                    name = "Mophun game",
                    spec = "application/octet-stream"
                }
                #endif
            }, (string path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    InstallGame(path);
                }
            });

            if (!permissionGranted)
            {
                Debug.Log("Todo: Show error message not granted");
            }
        }

        public void ImmediateShow()
        {
            gameObject.SetActive(true);
        }

        public void ImmediateHide()
        {
            gameObject.SetActive(false);
        }
    }
}
