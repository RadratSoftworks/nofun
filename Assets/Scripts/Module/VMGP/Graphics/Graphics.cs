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
using Nofun.Util;
using System;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private int foregroundColor;
        private int backgroundColor;
        private uint currentTransferMode = (uint)TransferMode.Transparent;

        private int GetSimpleSpriteRotation()
        {
            bool flipXSet = BitUtil.FlagSet(currentTransferMode, TransferMode.FlipX);
            bool flipYSet = BitUtil.FlagSet(currentTransferMode, TransferMode.FlipX);
        
            if (flipXSet && flipYSet)
            {
                return 270;
            }

            if (flipXSet)
            {
                return 180;
            }

            if (flipYSet)
            {
                return 90;
            }

            return 0;
        }

        [ModuleCall]
        private void vClearScreen(Int32 color)
        {
            system.GraphicDriver.ClearScreen(GetColor(color));
        }

        [ModuleCall]
        private void vSetBackColor(Int32 color)
        {
            backgroundColor = color;
        }

        [ModuleCall]
        private void vSetForeColor(Int32 color)
        {
            foregroundColor = color;
        }

        [ModuleCall]
        private uint vGetPaletteEntry(byte index)
        {
            return ScreenPalette[index].ToRgb555();
        }

        [ModuleCall]
        private void vSetPaletteEntry(byte index, uint rgb555Color)
        {
            ScreenPalette[index] = SColor.FromRgb555(rgb555Color);
        }

        [ModuleCall]
        private int vFindRGBIndex(uint rgb)
        {
            SColor colorMatch = SColor.FromRgb555(rgb);
            float diffSmallest = float.MaxValue;
            int targetIndex = 0;

            for (int i = 0; i < ScreenPalette.Length; i++)
            {
                float diff = colorMatch.Difference(ScreenPalette[i]);
                if (diff < diffSmallest)
                {
                    diffSmallest = diff;
                    targetIndex = i;
                }
            }

            return targetIndex;
        }

        [ModuleCall]
        private void vFlipScreen(Int32 block)
        {
            // This should always block
            system.GraphicDriver.FlipScreen();
        }

        [ModuleCall]
        private void vSetClipWindow(ushort x0, ushort y0, ushort x1, ushort y1)
        {
            system.GraphicDriver.SetClipRect(x0, y0, x1, y1);
        }

        [ModuleCall]
        private uint vSetTransferMode(uint transferMode)
        {
            uint previousMode = currentTransferMode;
            currentTransferMode = transferMode;

            return currentTransferMode;
        }

        [ModuleCall]
        private void vFillRect(short x0, short y0, short x1, short y1)
        {
            if ((x0 >= x1) || (y0 >= y1))
            {
                return;
            }

            system.GraphicDriver.FillRect(x0, y0, x1, y1, GetColor(foregroundColor));
        }

        [ModuleCall]
        private void vDrawLine(short x0, short y0, short x1, short y1)
        {
            system.GraphicDriver.DrawLine(x0, y0, x1, y1, GetColor(foregroundColor));
        }

        [ModuleCall]
        private void vUpdateSpriteMap()
        {
            vUpdateMap();
            vUpdateSprite();
        }
    }
}