using System;
using Nofun.Data.Model;
using UnityEngine.UIElements;

namespace Nofun.UI
{
    public class GameInfoEntryController
    {
        private VisualElement _iconElement;
        private Label _gameNameLabel;
        private GameIconManifest _gameIconManifest;
        private string _gamePath;

        public event Action<string> OnGameInfoChoosen;

        public GameInfoEntryController(GameIconManifest gameIconManifest)
        {
            _gameIconManifest = gameIconManifest;
        }

        public void SetVisualElement(VisualElement visualElement)
        {
            _iconElement = visualElement.Q("Icon");
            _gameNameLabel = visualElement.Q<Label>("Name");

            visualElement.RegisterCallback<MouseDownEvent>(_ => OnGameInfoChoosen?.Invoke(_gamePath));
        }

        public void BindData(GameInfo gameInfo)
        {
            _gameNameLabel.text = gameInfo.Name;
            _gamePath = gameInfo.GameFileName;

            var preloadedGameIcon = _gameIconManifest.FindGameIcon(gameInfo.Name);

            if (preloadedGameIcon != null)
            {
                _iconElement.style.backgroundImage = new StyleBackground(preloadedGameIcon.Icon);
            }
        }
    }
}
