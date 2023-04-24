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

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private class SpriteSlot
        {
            public VMPtr<NativeSprite> sprite;
            public short x;
            public short y;
        }

        private SpriteSlot[] spriteSlots;

        [ModuleCall]
        private uint vSpriteInit(byte count)
        {
            if ((spriteSlots != null) && (spriteSlots.Length == count))
            {
                return 1;
            }

            if (spriteSlots != null)
            {
                Logger.Warning(LogClass.VMGPGraphic, "Sprite slots have already been initialized! Re-allocating");
            }

            spriteSlots = new SpriteSlot[count];
            return 1;
        }

        [ModuleCall]
        private void vSpriteDispose()
        {
            spriteSlots = null;
        }

        [ModuleCall]
        private void vSpriteClear()
        {
            for (int i = 0; i < spriteSlots.Length; i++)
            {
                if (spriteSlots[i] != null)
                {
                    spriteSlots[i].sprite = VMPtr<NativeSprite>.Null;
                }
            }
        }

        [ModuleCall]
        private void vSpriteSet(byte slot, VMPtr<NativeSprite> sprite, short x, short y)
        {
            if (slot >= spriteSlots.Length)
            {
                Logger.Warning(LogClass.VMGPGraphic, $"Trying to set sprite slot={slot} to sprite array length={spriteSlots.Length}!");
                return;
            }

            SpriteSlot slotData = spriteSlots[slot];

            if (slotData == null)
            {
                slotData = new SpriteSlot()
                {
                    sprite = sprite,
                    x = x,
                    y = y
                };

                spriteSlots[slot] = slotData;
            }
            else
            {
                slotData.sprite = sprite;
                slotData.x = x;
                slotData.y = y;
            }
        }

        [ModuleCall]
        private void vUpdateSprite()
        {
            foreach (SpriteSlot slot in spriteSlots)
            {
                if (slot != null)
                {
                    vDrawObject(slot.x, slot.y, slot.sprite);
                }
            }
        }

        [ModuleCall]
        private short vSpriteBoxCollision(VMPtr<VMGPRect> boxPtr, byte from, byte to)
        {
            VMGPRect box = boxPtr.Read(system.Memory);
            NRectangle boxN = box.ToNofunRectangle();

            for (int i = from; i <= Math.Min((int)to, spriteSlots.Length - 1); i++)
            {
                if ((spriteSlots[i] == null) || (spriteSlots[i].sprite.IsNull))
                {
                    continue;
                }

                NativeSprite sprite = spriteSlots[i].sprite.Read(system.Memory);
                NRectangle colliderSprite = new NRectangle(spriteSlots[i].x, spriteSlots[i].y,
                    sprite.width, sprite.height);

                if (boxN.Collide(colliderSprite))
                {
                    return (short)i;
                }
            }

            return -1;
        }

        [ModuleCall]
        private void vDrawObject(short x, short y, VMPtr<NativeSprite> sprite)
        {
            if (sprite.IsNull)
            {
                return;
            }

            NativeSprite spriteInfo = sprite.Read(system.Memory);
            if ((spriteInfo.width == 0) || (spriteInfo.height == 0))
            {
                return;
            }

            long spriteSizeInBits = TextureUtil.GetTextureSizeInBits(spriteInfo.width, spriteInfo.height,
                (TextureFormat)spriteInfo.format);

            int spriteSizeInBytes = (int)((spriteSizeInBits + 7) / 8);

            Span<byte> spriteData = sprite[1].Cast<byte>().AsSpan(system.Memory, spriteSizeInBytes);

            ITexture drawTexture = spriteCache.Retrieve(system.GraphicDriver, spriteInfo, spriteData,
                ScreenPalette);

            // Draw it to the screen
            system.GraphicDriver.DrawTexture(x, y, spriteInfo.centerX, spriteInfo.centerY,
                GetSimpleSpriteRotation(), drawTexture,
                blackAsTransparent: BitUtil.FlagSet(currentTransferMode, TransferMode.Transparent));
        }
    }
}