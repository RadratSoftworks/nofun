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

namespace Nofun.Driver.Input
{
    public enum KeyCode : uint
    {
        Up = 0x1,
        Down = 0x2,
        Left = 0x4,
        Right = 0x8,
        Fire = 0x10,
        Select = 0x20,
        PointerDown = 0x40,
        PointerAltDown = 0x80,
        Fire2 = 0x100,

        SEJoystickPush = 0xD6
    }
}