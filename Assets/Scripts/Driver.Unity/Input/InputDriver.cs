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
using Nofun.Util;
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
            switch (keyCodeAsciiOrImplDefined)
            {
                case '1':
                    {
                        return ((buttonData & (uint)(Driver.Input.KeyCode.Up | Driver.Input.KeyCode.Left)) != 0);
                    }

                case '2':
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Up);
                    }

                case '3':
                    {
                        return ((buttonData & (uint)(Driver.Input.KeyCode.Up | Driver.Input.KeyCode.Right)) != 0);
                    }

                case '4':
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Left);
                    }

                case '5':
                case '*':
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Fire);
                    }

                case '#':
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Fire2);
                    }

                case '0':
                case (uint)SystemAsciiCode.SEOption:
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Select);
                    }

                case '6':
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Right);
                    }

                case '7':
                    {
                        return ((buttonData & (uint)(Driver.Input.KeyCode.Down | Driver.Input.KeyCode.Left)) != 0);
                    }

                case '8':
                    {
                        return BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Down);
                    }

                case '9':
                    {
                        return ((buttonData & (uint)(Driver.Input.KeyCode.Down | Driver.Input.KeyCode.Right)) != 0);
                    }

                default:
                    return false;
            }
        }

        public uint KeyScan
        {
            get
            {
                if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Up))
                {
                    if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Left))
                    {
                        return '1';
                    }

                    if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Right))
                    {
                        return '3';
                    }

                    return '2';
                }
                else if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Down))
                {
                    if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Left))
                    {
                        return '7';
                    }

                    if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Right))
                    {
                        return '9';
                    }

                    return '8';
                }
                else if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Left))
                {
                    return '4';
                }
                else if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Right))
                {
                    return '6';
                }
                else if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Fire))
                {
                    return '5';
                }
                else if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Fire2))
                {
                    return '*';
                }
                else if (BitUtil.FlagSet(buttonData, Driver.Input.KeyCode.Select))
                {
                    return (uint)SystemAsciiCode.SEOption;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void EndFrame()
        {
        }
    }
}