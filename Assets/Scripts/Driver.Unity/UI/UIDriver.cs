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
using UnityEngine;

using Nofun.Driver.UI;
using System.Threading;
using Nofun.Services;
using VContainer;

namespace Nofun.Driver.Unity.UI
{
    public class UIDriver : MonoBehaviour, IUIDriver
    {
        [Inject] private IDialogService dialogService;

        public void Show(Severity severity, string title, string message, ButtonType buttonType, Action<int> buttonPressed)
        {
            int passingValue = 0;
            AutoResetEvent evt = new AutoResetEvent(false);

            JobScheduler.Instance.RunOnUnityThread(() => dialogService.Show(severity, buttonType, title, message, (int value) =>
            {
                passingValue = value;
                evt.Set();
            }));

            evt.WaitOne();
            buttonPressed(passingValue);
        }
    }
}