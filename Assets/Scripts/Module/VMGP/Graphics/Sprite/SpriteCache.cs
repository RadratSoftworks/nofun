using Nofun.Driver.Graphics;
using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMGP
{
    internal class SpriteCacheEntry : ICacheEntry
    {
        public ITexture texture;

        public DateTime LastAccessed { get; set; }
    }

    internal class SpriteCache : Cache<SpriteCacheEntry>
    {
        public SpriteCache(int cacheLimit = 4096)
            : base(cacheLimit)
        {
        }

        private bool IsSpriteFormatSupported(TextureFormat format)
        {
            return (format == TextureFormat.RGB332);
        }

        public ITexture Retrieve(IGraphicDriver driver, NativeSprite spriteInfo, Span<byte> spriteData, Span<byte> paletteData)
        {
            if (!IsSpriteFormatSupported((TextureFormat)spriteInfo.format))
            {
                throw new UnimplementedFeatureException("Sprite other than RGB332 is not yet implemented!");
            }

            XxHash32 hasher = new();

            hasher.Append(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.format, 1));
            hasher.Append(MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.width, 1)));
            hasher.Append(MemoryMarshal.Cast<ushort, byte>(MemoryMarshal.CreateReadOnlySpan(ref spriteInfo.height, 1)));
            hasher.Append(spriteData);

            if (spriteInfo.paletteOffset != 0)
            {
                hasher.Append(paletteData);
            }

            byte[] hashResult = hasher.GetCurrentHash();
            uint hash = BitConverter.ToUInt32(hashResult);

            SpriteCacheEntry entry = GetFromCache(hash);

            if (entry != null)
            {
                return entry.texture;
            }

            // Should create a new one
            ITexture finalTex = driver.CreateTexture(spriteData.ToArray(), spriteInfo.width, spriteInfo.height, 1, (TextureFormat)spriteInfo.format);
            AddToCache(hash, new SpriteCacheEntry()
            {
                texture = finalTex,
                LastAccessed = DateTime.Now
            });

            return finalTex;
        }
    };
}