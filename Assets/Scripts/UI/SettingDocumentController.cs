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

using Nofun.Module.VMGPCaps;
using Nofun.Settings;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Nofun.Services;
using VContainer;

namespace Nofun.UI
{
    public class SettingDocumentController : FlexibleUIDocumentController
    {
        private static Dictionary<SystemDeviceModel, Tuple<int, int>> DeviceModelToScreenSizes = new()
        {
            { SystemDeviceModel.ArchosAV500, new Tuple<int, int>(480, 272) },
            { SystemDeviceModel.MotorolaA920, new Tuple<int, int>(208, 320) },
            { SystemDeviceModel.MotorolaA925, new Tuple<int, int>(208, 320) },
            { SystemDeviceModel.Nokia3650, new Tuple<int, int>(176, 208) },
            { SystemDeviceModel.Nokia6600, new Tuple<int, int>(176, 208) },
            { SystemDeviceModel.Nokia7650, new Tuple<int, int>(176, 208) },
            { SystemDeviceModel.NokiaNgage, new Tuple<int, int>(176, 208) },
            { SystemDeviceModel.SendoX, new Tuple<int, int>(176, 220) },
            { SystemDeviceModel.SiemensSX1, new Tuple<int, int>(176, 220) },
            { SystemDeviceModel.SonyEricssonT300, new Tuple<int, int>(101, 80) },
            { SystemDeviceModel.SonyEricssonT310, new Tuple<int, int>(101, 80) },
            { SystemDeviceModel.SonyEricssonT610, new Tuple<int, int>(128, 160) },
            { SystemDeviceModel.SonyErricisonP900, new Tuple<int, int>(208, 320) },
            { SystemDeviceModel.SonyErricssonP800, new Tuple<int, int>(208, 320) },
            { SystemDeviceModel.SonyErricssonT226, new Tuple<int, int>(80, 101) },
            { SystemDeviceModel.TigerTelematicGametrac, new Tuple<int, int>(480, 272) }
        };

        private const string SourceCodeURL = "https://github.com/RadratSoftworks/nofun";

        private TextField sizeXText;
        private TextField sizeYText;
        private TextField fpsField;

        private DropdownField screenModeDropdown;
        private DropdownField orientationDropdown;
        private DropdownField deviceDropdown;
        private DropdownField systemVersionDropdown;

        private Toggle softwareScissorCheck;

        private GameSettingsManager settingManager;
        private string gameName;
        private bool isFirst = false;

        [Inject] private ITranslationService translationService;
        [Inject] private IDialogService dialogService;

        private Button confirmButton;

        [SerializeField]
        private GameObject messageBoxPrefab;

        [SerializeField]
        private float fadeInOutDuration = 0.4f;

        public event Action Finished;

        public override void Awake()
        {
            base.Awake();

            DOTween.Init();

            VisualElement root = document.rootVisualElement;
            root.style.display = DisplayStyle.None;

            VisualElement screenSizeElem = root.Q("ScreenSize");
            sizeXText = screenSizeElem.Q<TextField>("XNumberInput");
            sizeYText = screenSizeElem.Q<TextField>("YNumberInput");

            screenModeDropdown = root.Q<DropdownField>("ScreenModeCombo");
            orientationDropdown = root.Q<DropdownField>("OrientationCombo");
            deviceDropdown = root.Q<DropdownField>("DeviceCombo");
            systemVersionDropdown = root.Q<DropdownField>("VersionCombo");
            softwareScissorCheck = root.Q<Toggle>("SoftwareScissorToggle");
            fpsField = root.Q<TextField>("FPSField");

            confirmButton = root.Q<Button>("ConfirmButton");
            Button cancelButton = root.Q<Button>("CancelButton");
            Button syncSizeButton = root.Q<Button>("SyncSizeButton");
            Button sourceCodeButton = root.Q<Button>("SourceCodeButton");

            confirmButton.clicked += OnOKButtonClicked;
            cancelButton.clicked += OnCancelButtonClicked;
            syncSizeButton.clicked += OnSyncSizeButtonClicked;

            sourceCodeButton.clicked += () =>
            {
                Application.OpenURL(SourceCodeURL);
            };

            if (!Application.isMobilePlatform)
            {
                // Hide it if not on mobile platform
                orientationDropdown.style.display = DisplayStyle.None;
            }
        }

        public void Setup(GameSettingsManager manager, string gameName)
        {
            this.settingManager = manager;
            this.gameName = gameName;

            VisualElement root = document.rootVisualElement;
            root.Q<Label>("GameLabel").text = gameName;
        }

        public void Show()
        {
            Load();

            VisualElement root = document.rootVisualElement;

            root.style.opacity = 0.0f;
            root.style.display = DisplayStyle.Flex;

            DOTween.To(() => root.style.opacity.value, value => root.style.opacity = (float)value, 1.0f, fadeInOutDuration);
        }

        public void Load()
        {
            GameSetting? setting = settingManager.Get(gameName);
            if (setting == null)
            {
                // Make some default value
                sizeXText.value = "240";
                sizeYText.value = "320";
                fpsField.value = "30";

                screenModeDropdown.index = (int)ScreenMode.CustomSize;
                deviceDropdown.index = (int)SystemDeviceModel.SonyEricssonT310;
                orientationDropdown.index = (int)Settings.ScreenOrientation.Potrait;
                systemVersionDropdown.index = (int)SystemVersion.Version150;

                softwareScissorCheck.value = false;

                isFirst = true;
                confirmButton.text = "Start";
            }
            else
            {
                sizeXText.value = setting.Value.screenSizeX.ToString();
                sizeYText.value = setting.Value.screenSizeY.ToString();
                fpsField.value = setting.Value.fps.ToString();

                // Most of them are straight-forward map
                screenModeDropdown.index = (int)setting.Value.screenMode;
                deviceDropdown.index = (int)setting.Value.deviceModel;
                orientationDropdown.index = (int)setting.Value.orientation;
                systemVersionDropdown.index = (int)setting.Value.systemVersion;

                softwareScissorCheck.value = setting.Value.enableSoftwareScissor;

                isFirst = false;
                confirmButton.text = "Save";
            }
        }

        public bool Save()
        {
            GameSetting newSetting = new();

            newSetting.screenSizeX = int.Parse(sizeXText.value);
            newSetting.screenSizeY = int.Parse(sizeYText.value);
            newSetting.deviceModel = (SystemDeviceModel)deviceDropdown.index;
            newSetting.screenMode = (ScreenMode)screenModeDropdown.index;
            newSetting.orientation = (Settings.ScreenOrientation)orientationDropdown.index;
            newSetting.systemVersion = (SystemVersion)systemVersionDropdown.index;
            newSetting.enableSoftwareScissor = softwareScissorCheck.value;
            newSetting.fps = int.Parse(fpsField.value);

            return settingManager.Set(gameName, newSetting);
        }

        private void DoCloseAnimation(bool saveAgain = false)
        {
            VisualElement root = document.rootVisualElement;
            root.style.opacity = 1.0f;

            DOTween.To(() => root.style.opacity.value, value => document.rootVisualElement.style.opacity = (float)value, 0.0f, fadeInOutDuration)
                .OnComplete(() =>
                {
                    root.style.display = DisplayStyle.None;

                    if (saveAgain)
                    {
                        dialogService.Show(
                            Severity.Info,
                            ButtonType.OK,
                            translationService.Translate("Settings_Saved"),
                            translationService.Translate("Settings_Saved_Details"),
                            value =>
                            {
                                Finished?.Invoke();
                            });
                    }
                    else
                    {
                        Finished?.Invoke();
                    }
                });
        }

        public void OnSyncSizeButtonClicked()
        {
            Tuple<int, int> dimension = DeviceModelToScreenSizes[(SystemDeviceModel)deviceDropdown.index];
            sizeXText.value = dimension.Item1.ToString();
            sizeYText.value = dimension.Item2.ToString();
        }

        public void OnCancelButtonClicked()
        {
            if (isFirst)
            {
                Application.Quit();
            }
            else
            {
                DoCloseAnimation();
            }
        }

        public void OnOKButtonClicked()
        {
            DoCloseAnimation(!Save());
        }
    }
}
