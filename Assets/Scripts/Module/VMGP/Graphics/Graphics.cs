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
using Nofun.Util.Logging;
using Nofun.VM;
using System;
using Nofun.Settings;
using Logger = Nofun.Util.Logging.Logger;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private int foregroundColor;
        private int backgroundColor;
        private uint flipCount;
        private uint currentTransferMode = (uint)TransferMode.Transparent;

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
        private void vSetPalette(VMPtr<ushort> colors, byte startIndex, ushort count)
        {
            count = Math.Min(count, (ushort)256);
            Span<ushort> colorSpan = colors.AsSpan(system.Memory, count);
            for (int i = 0; i < count; i++)
            {
                ScreenPalette[startIndex + i] = SColor.FromRgb555(colorSpan[i]);
            }
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
            flipCount++;
        }

        [ModuleCall]
        private uint vFrameTickCount()
        {
            return flipCount;
        }

        [ModuleCall]
        private int vWaitVBL(int block)
        {
            return 0;
        }

        [ModuleCall]
        private void vSetClipWindow(short x0, short y0, short x1, short y1)
        {
            system.GraphicDriver.ClipRect = new NRectangle(x0, y0, x1 - x0, y1 - y0);
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

            NRectangle clipRect = system.GraphicDriver.ClipRect;

            if (system.Version < SystemVersion.Version150)
            {
                system.GraphicDriver.ClipRect = new NRectangle(0, 0, system.GraphicDriver.ScreenWidth, system.GraphicDriver.ScreenHeight);
            }

            system.GraphicDriver.FillRect(x0, y0, x1, y1, GetColor(foregroundColor));

            if (system.Version < SystemVersion.Version150)
            {
                system.GraphicDriver.ClipRect = clipRect;
            }
        }

        [ModuleCall]
        private void vDrawLine(short x0, short y0, short x1, short y1)
        {
            system.GraphicDriver.DrawLine(x0, y0, x1, y1, GetColor(foregroundColor));
        }

        [ModuleCall]
        private void vDrawFlatPolygon(VMPtr<Triangle> triPtr)
        {
            Triangle tri = triPtr.Read(system.Memory);
            system.GraphicDriver.DrawTriangle(tri.x0, tri.y0, tri.x1, tri.y1, tri.x2, tri.y2, GetColor(foregroundColor));
        }

        [ModuleCall]
        private void vUpdateSpriteMap()
        {
            vUpdateMap();
            vUpdateSprite();
        }

        [ModuleCall]
        private void vSetDisplayWindow(int width, int height)
        {
            Logger.Trace(LogClass.VMGPGraphic, $"Set display window stubbed (width={width}, height={height})");
        }

        [ModuleCall]
        private void vPlot(short x, short y)
        {

        }

        [ModuleCall]
        private ushort vGetPixel(short x, short y)
        {
            return 0;
        }

        [ModuleCall]
        private void vCopyRect()
        {

        }
    }
}