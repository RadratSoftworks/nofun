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