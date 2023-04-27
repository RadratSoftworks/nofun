using Nofun.Driver.Graphics;
using Nofun.VM;
using System;
using System.IO.Hashing;
using System.Runtime.InteropServices;

using Nofun.Util;

namespace Nofun.Module.VMGP3D
{
    public class TextureCacheEntry : ICacheEntry
    {
        public ITexture nativeTexture;
        public DateTime LastAccessed { get; set; }
    }

    /// <summary>
    /// A texture cache, used to manage and detect texture set through vSetTexture.
    /// </summary>
    public class TextureCache : LTUFixedCapCache<TextureCacheEntry>
    {
        private static ITexture Upload(IGraphicDriver driver, Span<byte> textureData, TextureFormat format, uint lods, uint mipCount, Span<SColor> palettes)
        {
            return driver.CreateTexture(textureData.ToArray(), 1 << (int)(lods & 0xFF), 1 << (int)((lods >> 8) & 0xFF), (int)mipCount, format, palettes);
        }

        public static ITexture Upload(IGraphicDriver driver, VMPtr<byte> dataPtr, VMMemory memory, TextureFormat format, uint lods, uint mipCount, Span<SColor> palettes)
        {
            long totalSize = 0;
            int actualMipCount = 0;

            int lodX = (int)(lods & 0xFF);
            int lodY = (int)((lods >> 8) & 0xFF);

            for (int i = 0; (i < mipCount) && (lodY >= 0) && (lodY >= 0); i++)
            {
                totalSize += (TextureUtil.GetTextureSizeInBits(1 << lodX, 1 << lodY, format) + 7) >> 3;
                
                lodX -= 1;
                lodY -= 1;

                actualMipCount++;
            }

            return Upload(driver, dataPtr.AsSpan(memory, (int)totalSize), format, lods, (uint)actualMipCount, palettes);
        }

        public ITexture Get(IGraphicDriver driver, VMPtr<byte> dataPtr, VMMemory memory, TextureFormat format, uint lods, uint mipCount, Span<SColor> palettes)
        {
            XxHash32 hasher = new();

            int lodX = (int)(lods & 0xFF);
            int lodY = (int)((lods >> 8) & 0xFF);

            VMPtr<byte> hashDataPtr = dataPtr;
            long totalSize = 0;
            int actualMipCount = 0;

            for (int i = 0; (i < mipCount) && (lodY >= 0) && (lodY >= 0); i++)
            {
                long textureSizeThisLevel = (TextureUtil.GetTextureSizeInBits(1 << lodX, 1 << lodY, format) + 7) >> 3;
                hasher.Append(hashDataPtr.AsSpan(memory, (int)textureSizeThisLevel));

                hashDataPtr += (int)textureSizeThisLevel;
                totalSize += textureSizeThisLevel;

                lodX -= 1;
                lodY -= 1;

                actualMipCount++;
            }

            hasher.Append(MemoryMarshal.Cast<TextureFormat, byte>(MemoryMarshal.CreateReadOnlySpan(ref format, 1)));
            hasher.Append(MemoryMarshal.Cast<uint, byte>(MemoryMarshal.CreateReadOnlySpan(ref lods, 1)));
            hasher.Append(MemoryMarshal.Cast<int, byte>(MemoryMarshal.CreateReadOnlySpan(ref actualMipCount, 1)));

            uint hash = BitConverter.ToUInt32(hasher.GetCurrentHash());

            TextureCacheEntry entry = GetFromCache(hash);
            if (entry != null)
            {
                return entry.nativeTexture;
            }

            ITexture newTex = Upload(driver, dataPtr.AsSpan(memory, (int)totalSize), format, lods, (uint)actualMipCount, palettes);
            AddToCache(hash, new TextureCacheEntry()
            {
                LastAccessed = DateTime.Now,
                nativeTexture = newTex
            });

            return newTex;
        }
    }
}