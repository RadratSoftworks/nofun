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
using System.IO.Hashing;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMGP
{
    internal class SpriteCacheEntry : ICacheEntry
    {
        public ITexture texture;

        public DateTime LastAccessed { get; set; }
    }

    internal class SpriteCache : LTUFixedCapCache<SpriteCacheEntry>
    {
        public SpriteCache(int cacheLimit = 4096)
            : base(cacheLimit)
        {
        }

        private bool IsSpriteFormatSupported(TextureFormat format)
        {
            return (format >= TextureFormat.Palette2) && (format <= TextureFormat.RGB332);
        }

        public ITexture Retrieve(IGraphicDriver driver, NativeSprite spriteInfo, Span<byte> spriteData, SColor[] palettes)
        {
            TextureFormat format = (TextureFormat)spriteInfo.format;

            if (!IsSpriteFormatSupported(format))
            {
                throw new UnimplementedFeatureException("Sprite other than RGB332 is not yet implemented!");
            }

            bool isPalette = (format >= TextureFormat.Palette2) && (format <= TextureFormat.Palette256);

            XxHash32 hasher = new();

            hasher.Append(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.format, 1));
            hasher.Append(MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.width, 1)));
            hasher.Append(MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.height, 1)));
            
            if (isPalette)
            {
                hasher.Append(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.paletteOffset, 1));
            }

            hasher.Append(spriteData);

            byte[] hashResult = hasher.GetCurrentHash();
            uint hash = BitConverter.ToUInt32(hashResult);

            SpriteCacheEntry entry = GetFromCache(hash);

            if (entry != null)
            {
                return entry.texture;
            }

            // Should create a new one
            ITexture finalTex = driver.CreateTexture(spriteData.ToArray(), spriteInfo.width, spriteInfo.height, 1, (TextureFormat)spriteInfo.format,
                isPalette ? palettes.AsSpan(spriteInfo.paletteOffset) : new Span<SColor>());

            AddToCache(hash, new SpriteCacheEntry()
            {
                texture = finalTex,
                LastAccessed = DateTime.Now
            });

            return finalTex;
        }
    };
}