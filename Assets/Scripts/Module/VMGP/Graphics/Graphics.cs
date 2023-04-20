using Nofun.Driver.Graphics;
using Nofun.Driver.Unity.Graphics;
using Nofun.VM;
using System;
using System.Linq;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private SColor foregroundColor;
        private SColor backgroundColor;
        private TransferMode currentTransferMode = TransferMode.Transparent;

        private SpriteCache spriteCache;

        private int GetSimpleSpriteRotation()
        {
            switch (currentTransferMode)
            {
                case TransferMode.FlipX:
                    return 90;

                case TransferMode.FlipY:
                    return 180;

                default:
                    return 0;
            }
        }

        [ModuleCall]
        private void vClearScreen(Int32 color)
        {
            system.GraphicDriver.ClearScreen(GetColor(color));
        }

        [ModuleCall]
        private void vSetBackColor(Int32 color)
        {
            backgroundColor = GetColor(color);
        }

        [ModuleCall]
        private void vSetForeColor(Int32 color)
        {
            foregroundColor = GetColor(color);
        }

        [ModuleCall]
        private uint vGetPaletteEntry(byte index)
        {
            return ScreenPalette[index].ToRgb555();
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
        private void vSetTransferMode(TransferMode transferMode)
        {
            currentTransferMode = transferMode;
        }

        [ModuleCall]
        private void vFillRect(short x0, short y0, short x1, short y1)
        {
            if ((x0 >= x1) || (y0 >= y1))
            {
                return;
            }

            system.GraphicDriver.FillRect(x0, y0, x1, y1, foregroundColor);
        }

        [ModuleCall]
        private void vDrawObject(short x, short y, VMPtr<NativeSprite> sprite)
        {
            NativeSprite spriteInfo = sprite.Read(system.Memory);
            long spriteSizeInBits = TextureUtil.GetTextureSizeInBits(spriteInfo.width, spriteInfo.height,
                (TextureFormat)spriteInfo.format);

            int spriteSizeInBytes = (int)((spriteSizeInBits + 7) / 8);

            Span<byte> spriteData = sprite[1].Cast<byte>().AsSpan(system.Memory, spriteSizeInBytes);
            Span<byte> paletteData = new Span<byte>();

            ITexture drawTexture = spriteCache.Retrieve(system.GraphicDriver, spriteInfo, spriteData,
                paletteData);

            // Draw it to the screen
            system.GraphicDriver.DrawTexture(x, y, spriteInfo.centerX, spriteInfo.centerY,
                GetSimpleSpriteRotation(), drawTexture);
        }
    }
}