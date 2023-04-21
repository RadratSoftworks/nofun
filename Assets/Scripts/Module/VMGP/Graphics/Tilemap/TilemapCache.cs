using Nofun.Driver.Graphics;
using System;

namespace Nofun.Module.VMGP
{
    internal class TilemapCacheEntry : ICacheEntry
    {
        public uint tileCount;
        public bool invalidated = false;

        public ITexture texture;
        public DateTime LastAccessed { get; set; }
    }

    /// <summary>
    /// A tilemap cache, store and allow retrieval of tilemap atlas.
    /// 
    /// The cache works on the assumption that the tilemap data is immutable.
    /// </summary>
    internal class TilemapCache : Cache<TilemapCacheEntry>
    {
        private const int TileMaxCount = 256;

        public TilemapCache(int cacheLimit = 4096)
            : base(cacheLimit)
        {

        }

        private bool IsTilemapDataSupported(TextureFormat format)
        {
            return (format == TextureFormat.RGB332);
        }

        public static void GetTilePositionInAtlas(int tileIndex, out int x, out int y)
        {
            int row = tileIndex / 32;
            int col = tileIndex % 32;

            x = col * 8;
            y = row * 8;
        }

        /// <summary>
        /// Mark a cache entry as needed to reupdate atlas.
        /// </summary>
        public void Invalidate(uint tileSpriteDataAddr)
        {
            TilemapCacheEntry entry = GetFromCache(tileSpriteDataAddr);
            if (entry != null)
            {
                entry.invalidated = true;
            }
        }

        public ITexture Retrive(IGraphicDriver driver, NativeMapHeader header, uint tileSpriteDataAddr, Span<byte> tileSpriteData, int tileMaxCount)
        {
            TextureFormat tileMapFormat = (TextureFormat)header.format;

            // Try to make a hash of the tilemap and look it up
            if (!IsTilemapDataSupported(tileMapFormat))
            {
                throw new UnimplementedFeatureException("Tilemap other than RGB332 is not yet implemented!");
            }

            TilemapCacheEntry entry = GetFromCache(tileSpriteDataAddr);

            if (entry != null)
            {
                if (!entry.invalidated && (entry.tileCount > tileMaxCount))
                {
                    return entry.texture;
                }
            }

            // Make the atlas
            ITexture resultTexture = entry?.texture;

            // Byte-streamable, copy by byte-row
            // Tile should be in 8x8 dimension. This is hardcoded
            if ((tileMapFormat == TextureFormat.Palette256) || (tileMapFormat == TextureFormat.RGB332))
            {
                byte[] dataUpload = new byte[256 * 8 * 8];

                // Divide the tile into rows
                // Each row can only contain 32 tile (= 32x8 = 256 pixels)
                for (int i = 0; i < tileMaxCount; i++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        tileSpriteData.Slice(i * 64 + y * 8, 8).CopyTo(new Span<byte>(dataUpload, ((i >> 5) * 256 * 8) + y * 256 + (i & 31) * 8, 8));
                    }
                }

                if (resultTexture != null)
                {
                    resultTexture.SetData(dataUpload, 0);
                    resultTexture.Apply();
                }
                else
                {
                    resultTexture = driver.CreateTexture(dataUpload, 256, 64, 1, TextureFormat.RGB332);
                }
            }

            if (entry != null)
            {
                entry.invalidated = false;
                entry.tileCount = (uint)tileMaxCount;
            }
            else
            {
                AddToCache(tileSpriteDataAddr, new TilemapCacheEntry()
                {
                    texture = resultTexture,
                    tileCount = (uint)tileMaxCount,
                    invalidated = false
                });
            }

            return resultTexture;
        }
    };
}