using Nofun.Driver.Graphics;
using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMGP
{
    public class SpriteCache
    {
        private class CacheEntry
        {
            public ITexture texture;
            public DateTime lastAccesed;
        }

        private Dictionary<uint, CacheEntry> cache;
        private int cacheLimit;

        public SpriteCache(int cacheLimit = 4096)
        {
            this.cacheLimit = cacheLimit;
            this.cache = new();
        }

        private bool IsSpriteFormatSupported(TextureFormat format)
        {
            return (format == TextureFormat.RGB332);
        }

        /// <summary>
        /// Purge half of the cache, sorted by oldest time since used.
        /// </summary>
        private void Purge()
        {
            var purgeList = cache.OrderBy(x => x.Value.lastAccesed).Select(x => x.Key).ToList();
            for (int i = 0; i < purgeList.Count / 2; i++)
            {
                cache.Remove(purgeList[i]);
            }
        }

        public ITexture Retrieve(IGraphicDriver driver, NativeSprite spriteInfo, Span<byte> spriteData, Span<byte> paletteData)
        {
            if (!IsSpriteFormatSupported((TextureFormat)spriteInfo.format))
            {
                throw new UnimplementedFeatureException("Sprite other than RGB332 is not yet implemented!");
            }

            XxHash32 hasher = new();

            spriteInfo.centerX = 0;
            spriteInfo.centerY = 0;

            byte paletteOffset = spriteInfo.paletteOffset;
            spriteInfo.paletteOffset = 0;

            hasher.Append(MemoryMarshal.Cast<NativeSprite, byte>(MemoryMarshal.CreateSpan(ref spriteInfo, 1)));
            hasher.Append(spriteData);

            if (paletteOffset != 0)
            {
                hasher.Append(paletteData);
            }

            byte[] hashResult = hasher.GetCurrentHash();
            uint hash = BitConverter.ToUInt32(hashResult);

            if (cache.TryGetValue(hash, out CacheEntry tex))
            {
                tex.lastAccesed = DateTime.Now;
                return tex.texture;
            }

            if (cache.Count == cacheLimit)
            {
                Purge();
            }

            // Should create a new one
            ITexture finalTex = driver.CreateTexture(spriteData.ToArray(), spriteInfo.width, spriteInfo.height, 1, (TextureFormat)spriteInfo.format);
            cache.Add(hash, new CacheEntry()
            {
                texture = finalTex,
                lastAccesed = DateTime.Now
            });

            return finalTex;
        }
    };
}