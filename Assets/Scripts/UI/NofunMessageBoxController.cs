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

using System;
using Nofun.Driver.UI;
using DG.Tweening;

namespace Nofun.UI
{
    public class NofunMessageBoxController : FlexibleUIDocumentController
    {
        [SerializeField]
        private float popInOutDuration = 0.7f;

        private Button leftButton;
        private Button rightButton;
        private Button leftFarButton;
        private Label titleLabel;
        private Label contentLabel;
        private VisualElement root;

        private Action<int> pendingAction;

        public static void Show(GameObject boxPrefab, IUIDriver.Severity severity, IUIDriver.ButtonType buttonType, string title, string content, Action<int> buttonSubmitAct, float? customSortingOrder = null)
        {
            GameObject messageBox = Instantiate(boxPrefab);
            NofunMessageBoxController messageBoxController = messageBox.GetComponent<NofunMessageBoxController>();

            if (customSortingOrder != null)
            {
                UIDocument document = messageBox.GetComponent<UIDocument>();
                document.sortingOrder = customSortingOrder.Value;
            }

            messageBoxController.Show(severity, title, content, buttonType,
                value => {
                    Destroy(messageBox);
                    buttonSubmitAct?.Invoke(value);
                });
        }

        public override void Awake()
        {
            base.Awake();

            root = document.rootVisualElement;

            titleLabel = root.Q<Label>("TitleValue");
            contentLabel = root.Q<Label>("TextValue");

            leftFarButton = root.Q<Button>("LeftFar");
            leftButton = root.Q<Button>("Left");
            rightButton = root.Q<Button>("Right");

            leftFarButton.clicked += OnLeftFarButtonClicked;
            leftButton.clicked += OnLeftButtonClicked;
            rightButton.clicked += OnRightButtonClicked;

            leftFarButton.style.display = leftButton.style.display = rightButton.style.display = DisplayStyle.None;
            root.style.display = DisplayStyle.None;

            DOTween.Init();
        }

        private void SubmitAndClose(int value)
        {
            DOTween.To(() => root.transform.scale, value => root.transform.scale = value, Vector3.zero, popInOutDuration)
                .SetEase(Ease.InOutBack)
                .OnComplete(() =>
            {
                root.style.display = DisplayStyle.None;

                pendingAction?.Invoke(value);
                pendingAction = null;
            });
        }

        private void OnLeftFarButtonClicked()
        {
            SubmitAndClose(2);
        }

        private void OnLeftButtonClicked()
        {
            SubmitAndClose(1);
        }

        private void OnRightButtonClicked()
        {
            SubmitAndClose(0);
        }

        public void Show(IUIDriver.Severity severity, string title, string message, IUIDriver.ButtonType buttonType, Action<int> buttonPressed)
        {
            if (buttonPressed == null)
            {
                throw new ArgumentNullException("Handle button pressed action must be present");
            }

            if (pendingAction != null)
            {
                throw new InvalidOperationException("Cannot show message box while another is already showing");
            }

            // TODO: Not ignoring severity
            if (title != null)
            {
                titleLabel.text = title;
                titleLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                titleLabel.style.display = DisplayStyle.None;
            }

            contentLabel.text = message;

            switch (buttonType)
            {
                case IUIDriver.ButtonType.OK:
                {
                    leftFarButton.style.display = DisplayStyle.None;
                    leftButton.style.display = DisplayStyle.None;
                    rightButton.style.display = DisplayStyle.Flex;

                    rightButton.text = "OK";
                    break;
                }

                case IUIDriver.ButtonType.OKCancel:
                {
                    leftFarButton.style.display = DisplayStyle.None;
                    leftButton.style.display = DisplayStyle.Flex;
                    rightButton.style.display = DisplayStyle.Flex;

                    leftButton.text = "Cancel";
                    rightButton.text = "OK";
                    break;
                }

                case IUIDriver.ButtonType.YesNo:
                {
                    leftFarButton.style.display = DisplayStyle.None;
                    leftButton.style.display = DisplayStyle.Flex;
                    rightButton.style.display = DisplayStyle.Flex;

                    leftButton.text = "No";
                    rightButton.text = "Yes";
                    break;
                }

                case IUIDriver.ButtonType.YesNoCancel:
                {
                    leftFarButton.style.display = DisplayStyle.Flex;
                    leftButton.style.display = DisplayStyle.Flex;
                    rightButton.style.display = DisplayStyle.Flex;

                    leftButton.text = "Cancel";
                    rightButton.text = "Yes";
                    break;
                }

                default:
                {
                    throw new ArgumentException("Invalid button type");
                }
            }

            root.style.display = DisplayStyle.Flex;
            root.transform.scale = Vector3.zero;

            DOTween.To(() => root.transform.scale, value => root.transform.scale = value, Vector3.one, popInOutDuration)
                .SetEase(Ease.InOutBack)
                .OnComplete(() =>
            {
                pendingAction = buttonPressed;
            });
        }
    }
}
