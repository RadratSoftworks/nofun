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

using Nofun.Driver.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nofun.Driver.Unity.Input
{
    public class InputDriver : MonoBehaviour, IInputDriver
    {
        private uint buttonData;

        public void OnFire(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Fire;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Fire;
            }
        }

        public void OnFire2(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Fire2;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Fire2;
            }
        }

        public void OnLeft(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Left;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Left;
            }
        }

        public void OnRight(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Right;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Right;
            }
        }

        public void OnUp(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Up;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Up;
            }
        }

        public void OnDown(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Down;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Down;
            }
        }

        public void OnSelect(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.Select;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.Select;
            }
        }

        public void OnSEJoystickPush(InputValue value)
        {
            if (value.isPressed)
            {
                buttonData |= (uint)Driver.Input.KeyCode.SEJoystickPush;
            }
            else
            {
                buttonData &= ~(uint)Driver.Input.KeyCode.SEJoystickPush;
            }
        }

        public uint GetButtonData()
        {
            return buttonData;
        }

        public bool KeyPressed(uint keyCodeAsciiOrImplDefined)
        {
            return false;
        }

        public void EndFrame()
        {
        }
    }
}