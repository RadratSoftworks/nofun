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
using UnityEngine.UIElements;
using VContainer;

namespace Nofun.UI
{
    public class FlexibleUIDocumentController : MonoBehaviour
    {
        [Inject] protected ScreenManager screenManager;
        protected UIDocument document;

        private void UpdatePanelSettings()
        {
            document.panelSettings = screenManager.CurrentPanelSettings;
        }

        private void OnOrientationChanged(Settings.ScreenOrientation newOrientation)
        {
            UpdatePanelSettings();
        }

        public virtual void Awake()
        {
            document = GetComponent<UIDocument>();
        }

        public virtual void Start()
        {
            UpdatePanelSettings();
            screenManager.ScreenOrientationChanged += OnOrientationChanged;
        }
    }
}
