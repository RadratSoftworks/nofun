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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Nofun
{
    public class ScreenManager: MonoBehaviour
    {
        [SerializeField]
        private Canvas canvasPotrait;

        [SerializeField]
        private Canvas canvasLandscape;

        [SerializeField]
        private GameObject controlMobilePotrait;

        [SerializeField]
        private GameObject controlMobileLandscape;

        [SerializeField]
        private RawImage displayPotrait;

        [SerializeField]
        private RawImage displayLandscape;

        [SerializeField]
        private PanelSettings landscapePanelSettings;

        [SerializeField]
        private PanelSettings potraitPanelSettings;

        public static ScreenManager Instance { get; private set; }
        private Settings.ScreenOrientation screenOrientation;

        public event System.Action<Settings.ScreenOrientation> ScreenOrientationChanged;

        public Settings.ScreenOrientation ScreenOrientation
        {
            get => screenOrientation;
            set
            {
                SetScreenOrientationDetail(value);
            }
        }

        public RawImage CurrentDisplay => (screenOrientation == Settings.ScreenOrientation.Potrait) ? displayPotrait : displayLandscape;

        public PanelSettings CurrentPanelSettings => (screenOrientation == Settings.ScreenOrientation.Potrait) ? potraitPanelSettings : landscapePanelSettings;
        public Canvas CurrentCanvas => (screenOrientation == Settings.ScreenOrientation.Potrait) ? canvasPotrait : canvasLandscape;

        private void UpdateCanvasOrientation()
        {
            if (screenOrientation == Settings.ScreenOrientation.Potrait)
            {
                canvasPotrait.gameObject.SetActive(true);
                canvasLandscape.gameObject.SetActive(false);
            }
            else
            {
                canvasPotrait.gameObject.SetActive(false);
                canvasLandscape.gameObject.SetActive(true);
            }

            ScreenOrientationChanged?.Invoke(screenOrientation);
        }

        private void SetScreenOrientationDetail(Settings.ScreenOrientation value)
        {
            if (screenOrientation == value)
            {
                return;
            }

            if (Application.isMobilePlatform)
            {
                Screen.orientation = (value == Settings.ScreenOrientation.Potrait) ? UnityEngine.ScreenOrientation.Portrait : UnityEngine.ScreenOrientation.LandscapeLeft;
                screenOrientation = value;

                UpdateCanvasOrientation();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

#if !UNITY_EDITOR
            if (Application.isMobilePlatform)
            {
                screenOrientation = (Screen.orientation == UnityEngine.ScreenOrientation.Portrait) ? Settings.ScreenOrientation.Potrait : Settings.ScreenOrientation.Landscape;
            }
            else
#endif
            {
                if (Screen.width > Screen.height)
                {
                    screenOrientation = Settings.ScreenOrientation.Landscape;
                }
                else
                {
                    screenOrientation = Settings.ScreenOrientation.Potrait;
                }
            }
        }

        public void Start()
        {
            if (!Application.isMobilePlatform)
            {
                controlMobileLandscape.SetActive(false);
                controlMobilePotrait.SetActive(false);
            }

            UpdateCanvasOrientation();
        }
    }
}