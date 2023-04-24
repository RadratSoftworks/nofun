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

namespace Nofun.Driver.Audio
{
    public enum SoundCapsFlags : uint
    {
        Beep = 0x1,
        Wave = 0x2,
        Stereo = 0x4,
        Midi = 0x8,
        Mono = 0x10,
        EightBitPCM = 0x20,
        SixteenBitPCM = 0x40,
        CtrlFrequency = 0x80,
        CtrlPan = 0x100,
        CtrlVolume = 0x200,
        CtrlMasterVolume = 0x400,
        All = Beep | Wave | Stereo | Midi | Mono | EightBitPCM | SixteenBitPCM |
            CtrlFrequency | CtrlPan | CtrlVolume | CtrlMasterVolume
    }
}