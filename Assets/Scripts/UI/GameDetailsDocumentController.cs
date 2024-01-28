using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using Nofun.Data.Model;
using Nofun.Parser;
using Nofun.Settings;
using Nofun.Util.Unity;
using Nofun.VM;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nofun.UI
{
    public class GameDetailsDocumentController : FlexibleUIDocumentController
    {
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private SettingDocumentController settingDocumentController;

        private Label gameNameLabel;
        private Label vendorLabel;
        private Label versionLabel;
        private VisualElement gameIcon;

        private Button playButton;
        private Button pinToScreenButton;
        private Button uninstallButton;
        private Button settingButton;

        private GameInfo activeGameInfo;
        private VisualElement outerContainer;

        public Action<string> OnGameInfoChoosen;
        public Action<GameInfo> OnGameRemovalRequested;
        private GameSettingsManager gameSettingsManager;
        private IGameProvider gameProvider;

        public override void Awake()
        {
            base.Awake();

            gameNameLabel = document.rootVisualElement.Q<Label>("GameName");
            vendorLabel = document.rootVisualElement.Q<Label>("GameVendor");
            versionLabel = document.rootVisualElement.Q<Label>("GameVersion");
            gameIcon = document.rootVisualElement.Q<VisualElement>("GameIcon");

            playButton = document.rootVisualElement.Q<Button>("GameStartButton");
            pinToScreenButton = document.rootVisualElement.Q<Button>("PinToScreenButton");
            uninstallButton = document.rootVisualElement.Q<Button>("RemoveButton");
            settingButton = document.rootVisualElement.Q<Button>("SettingsButton");

            playButton.RegisterCallback<PointerUpEvent>(OnPlayButtonClicked);
            pinToScreenButton.RegisterCallback<PointerUpEvent>(OnPinToScreenButtonClicked);
            uninstallButton.RegisterCallback<PointerUpEvent>(OnUninstallButtonClicked);
            settingButton.RegisterCallback<PointerUpEvent>(OnSettingButtonClicked);

            outerContainer = document.rootVisualElement.Q<VisualElement>("Container");
            outerContainer.RegisterCallback<PointerDownEvent>(OnOuterDialogueClicked);

            document.rootVisualElement.style.display = DisplayStyle.None;
        }

        public void Setup(GameSettingsManager gameSettingsManager, IGameProvider gameProvider)
        {
            this.gameSettingsManager = gameSettingsManager;
            this.gameProvider = gameProvider;
        }

        private void OnOuterDialogueClicked(PointerDownEvent evt)
        {
            if (evt.target == outerContainer)
            {
                Hide(null);
            }
        }

        private void OnPlayButtonClicked(PointerUpEvent evt)
        {
            Hide(() => OnGameInfoChoosen?.Invoke(activeGameInfo.GameFileName));
        }

        private void OnPinToScreenButtonClicked(PointerUpEvent evt)
        {
        }

        private void OnUninstallButtonClicked(PointerUpEvent evt)
        {
            Hide(() => OnGameRemovalRequested?.Invoke(activeGameInfo));
        }

        private void OnSettingButtonClicked(PointerUpEvent evt)
        {
            using (FileStream stream = File.OpenRead(gameProvider.GetGamePath(activeGameInfo.GameFileName)))
            {
                VMGPExecutable executable = new VMGPExecutable(stream);

                settingDocumentController.Setup(gameSettingsManager, activeGameInfo.Name, VMSystem.GetSuitableDefaultSetting(executable));
                settingDocumentController.Show();
            }
        }

        private IEnumerator FinalizeIcon(StyleBackground gameIconTexture)
        {
            yield return null;

            gameIcon.style.width = gameIcon.resolvedStyle.height;
            gameIcon.style.backgroundImage = gameIconTexture;
        }

        public void Show(GameInfo gameInfo, StyleBackground gameIconTexture)
        {
            activeGameInfo = gameInfo;

            gameNameLabel.text = gameInfo.Name;
            vendorLabel.text = gameInfo.Vendor;
            versionLabel.text = $"{gameInfo.Major}.{gameInfo.Minor}.{gameInfo.Revision}";

            StartCoroutine(FinalizeIcon(gameIconTexture));

            documentStackManager.Push(this);
        }

        public void Hide(Action onComplete)
        {
            documentStackManager.Pop(this, onComplete);
        }
    }
}
