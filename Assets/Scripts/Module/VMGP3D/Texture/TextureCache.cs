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
        public override DateTime LastAccessed { get; set; }
    }

    /// <summary>
    /// A texture cache, used to manage and detect texture set through vSetTexture.
    /// </summary>
    public class TextureCache : LTUFixedCapCache<TextureCacheEntry>
    {
        private static ITexture Upload(IGraphicDriver driver, Span<byte> textureData, TextureFormat format, uint lods, uint mipCount, Memory<SColor> palettes)
        {
            byte[] dataFinal = textureData.ToArray();
            int width = 1 << (int)(lods & 0xFF);
            int height = 1 << (int)((lods >> 8) & 0xFF);

            VMGP3D.ResolveMophunSWTransparency(dataFinal, width, height, (int)mipCount, format);
            return driver.CreateTexture(dataFinal, width, height, (int)mipCount, format, palettes, true);
        }

        public static ITexture Upload(IGraphicDriver driver, VMPtr<byte> dataPtr, VMMemory memory, TextureFormat format, uint lods, uint mipCount, Memory<SColor> palettes)
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

        public ITexture Get(IGraphicDriver driver, VMPtr<byte> dataPtr, VMMemory memory, TextureFormat format, uint lods, uint mipCount, Memory<SColor> palettes)
        {
            // Does not include top lod
            mipCount += 1;

            XxHash32 hasher = new();

            int lodX = (int)(lods & 0xFF);
            int lodY = (int)((lods >> 8) & 0xFF);

            VMPtr<byte> hashDataPtr = dataPtr;
            long totalSize = 0;
            int actualMipCount = 0;

            byte highestPaletteIndex = 0;
            bool isPalette = TextureUtil.IsPaletteFormat(format);
            byte bitsSize = (byte)TextureUtil.GetPixelSizeInBits(format);

            for (int i = 0; (i < mipCount) && (lodY >= 0) && (lodY >= 0); i++)
            {
                long textureSizeThisLevel = (TextureUtil.GetTextureSizeInBits(1 << lodX, 1 << lodY, format) + 7) >> 3;
                Span<byte> data = hashDataPtr.AsSpan(memory, (int)textureSizeThisLevel);

                hasher.Append(data);

                if (isPalette)
                {
                    highestPaletteIndex = DataConvertor.FindHighestPaletteIndex(data, 1 << lodX, 1 << lodY, bitsSize);
                }

                hashDataPtr += (int)textureSizeThisLevel;
                totalSize += textureSizeThisLevel;

                lodX -= 1;
                lodY -= 1;

                actualMipCount++;
            }

            if (isPalette)
            {
                hasher.Append(MemoryMarshal.Cast<SColor, byte>(palettes.Span.Slice(0, highestPaletteIndex + 1)));
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