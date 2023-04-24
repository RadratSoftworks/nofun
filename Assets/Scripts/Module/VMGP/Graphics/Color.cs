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

using Nofun.Driver.Graphics;
using System;

namespace Nofun.Module.VMGP
{
    public partial class VMGP
    {
        public SColor GetColor(Int32 colorOrPalIndex)
        {
            // Bit 31 set, accidentally be a color
            if (colorOrPalIndex < 0)
            {
                ushort rgb555 = (ushort)(colorOrPalIndex & 0xFFFF);
                return SColor.FromRgb555(rgb555);
            }

            return ScreenPalette[Math.Clamp(colorOrPalIndex, 0, 255)];
        }
    }
}