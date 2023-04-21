using Nofun.Driver.Graphics;
using Nofun.Util;
using Nofun.VM;
using System;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        [ModuleCall]
        private void vDrawObject(short x, short y, VMPtr<NativeSprite> sprite)
        {
            NativeSprite spriteInfo = sprite.Read(system.Memory);
            if ((spriteInfo.width == 0) || (spriteInfo.height == 0))
            {
                return;
            }

            long spriteSizeInBits = TextureUtil.GetTextureSizeInBits(spriteInfo.width, spriteInfo.height,
                (TextureFormat)spriteInfo.format);

            int spriteSizeInBytes = (int)((spriteSizeInBits + 7) / 8);

            Span<byte> spriteData = sprite[1].Cast<byte>().AsSpan(system.Memory, spriteSizeInBytes);
            Span<byte> paletteData = new Span<byte>();

            ITexture drawTexture = spriteCache.Retrieve(system.GraphicDriver, spriteInfo, spriteData,
                paletteData);

            // Draw it to the screen
            system.GraphicDriver.DrawTexture(x, y, spriteInfo.centerX, spriteInfo.centerY,
                GetSimpleSpriteRotation(), drawTexture,
                blackAsTransparent: BitUtil.FlagSet(currentTransferMode, TransferMode.Transparent));
        }
    }
}