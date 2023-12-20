using System;
using DG.Tweening;
using Nofun.Data.Model;
using Nofun.DynamicIcons;
using UnityEngine.UIElements;

namespace Nofun.UI
{
    public class GameInfoEntryController
    {
        private readonly GameDetailsDocumentController gameDetailsDocumentController;

        private VisualElement iconElement;
        private Label gameNameLabel;
        private readonly GameIconManifest gameIconManifest;
        private readonly DynamicIconsProvider dynamicIconsProvider;
        private string gamePath;
        private Sequence holdEvent;
        private GameInfo gameInfo;

        public event Action<string> OnGameInfoChoosen;

        public GameInfoEntryController(GameIconManifest gameIconManifest, DynamicIconsProvider dynamicIconsProvider,
            GameDetailsDocumentController gameDetailsDocumentController)
        {
            this.gameIconManifest = gameIconManifest;
            this.gameDetailsDocumentController = gameDetailsDocumentController;
            this.dynamicIconsProvider = dynamicIconsProvider;
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            iconElement = visualElement.Q("Icon");
            gameNameLabel = visualElement.Q<Label>("Name");

            visualElement.RegisterCallback<PointerUpEvent>(_ =>
            {
                if (holdEvent != null)
                {
                    holdEvent.Kill();
                    holdEvent = null;
                }

                OnGameInfoChoosen?.Invoke(gamePath);
            });

            visualElement.RegisterCallback<PointerDownEvent>(_ =>
            {
                holdEvent = DOTween.Sequence()
                    .AppendInterval(0.5f)
                    .OnComplete(() =>
                    {
                        OnGameHeld();
                        holdEvent = null;
                    });
            });
        }

        private void OnGameHeld()
        {
            var icon = ResolveIcon(gameInfo);
            gameDetailsDocumentController.Show(gameInfo, icon);
        }

        private StyleBackground ResolveIcon(GameInfo gameInfo)
        {
            var preloadedGameIcon = gameIconManifest.FindGameIcon(gameInfo.Name);

            if (preloadedGameIcon != null)
            {
                return new StyleBackground(preloadedGameIcon.Icon);
            }
            else
            {
                var preloadedDynamicGameIcon = gameIconManifest.FindDynamicGameIcon(gameInfo.Name);
                if (preloadedDynamicGameIcon != null)
                {
                    var dynamicGameIcon = dynamicIconsProvider.GetIcon(preloadedDynamicGameIcon);
                    return new StyleBackground(Background.FromRenderTexture(dynamicGameIcon));
                }
            }

            return null;
        }

        public void BindData(GameInfo gameInfo)
        {
            this.gameInfo = gameInfo;

            gameNameLabel.text = gameInfo.Name;
            gamePath = gameInfo.GameFileName;

            iconElement.style.backgroundImage = ResolveIcon(gameInfo);
        }
    }
}
