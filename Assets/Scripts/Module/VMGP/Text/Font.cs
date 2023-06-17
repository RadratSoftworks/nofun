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
using Nofun.VM;
using Nofun.Util;

using System.Collections.Generic;
using System;
using System.Linq;
using System.IO.Hashing;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMGP
{
    public class Font
    {
        // The maximum amount of characters that a font can hold is 256.
        // So we may spend 16 row of bitmap characters.
        private const int CharPerAtlasRow = 16;
        private const int CharPerAtlasColumn = 16;

        private VMGPFont nativeFont;

        private Dictionary<char, int> charIndexInAtlas;
        private byte[] textureData;
        private SColor[] palette;

        private int AtlasWidth => nativeFont.width * CharPerAtlasRow;
        private int AtlasHeight => nativeFont.height * CharPerAtlasColumn;
        private int AtlasByteWidth => AtlasWidth * 4;
        public VMGPFont NativeFont => nativeFont;
        private HashSet<int> usedPalettes = new();
        private int largestCharIndex = 0;
        private LTUGeneralFixedCapCache<ITexture> atlasCache;

        public Font(VMGPFont nativeFont, SColor[] palette)
        {
            if ((nativeFont.bpp != 1) && (nativeFont.bpp != 2))
            {
                throw new InvalidOperationException($"Font with bpp={nativeFont.bpp} is not supported!");
            }

            this.nativeFont = nativeFont;
            this.palette = palette;
            this.charIndexInAtlas = new();
            this.atlasCache = new();

            textureData = new byte[AtlasByteWidth * AtlasHeight];
        }

        private UInt32 GetCurrentUseHash(VMMemory memory)
        {
            XxHash32 hasher = new();

            if (nativeFont.bpp > 1)
            {
                foreach (var index in usedPalettes)
                {
                    hasher.Append(MemoryMarshal.Cast<SColor, byte>(MemoryMarshal.CreateReadOnlySpan(ref palette[index], 1)));
                }
            }

            // Hash the bitmap data
            int charSize = MemoryUtil.AlignUp(nativeFont.width * nativeFont.height * nativeFont.bpp, 8) / 8;
            int dataHashSize = charSize * (largestCharIndex + 1);

            hasher.Append(nativeFont.fontData.AsSpan(memory, dataHashSize));
            return BitConverter.ToUInt32(hasher.GetCurrentHash());
        }

        private void UpdateAtlasData2BitPalette(Span<byte> charData, int destCharIndex)
        {
            int destLine = (destCharIndex / CharPerAtlasRow) * nativeFont.height;
            int destColumn = (destCharIndex % CharPerAtlasColumn) * nativeFont.width;

            int pixelIterated = 0;

            for (int y = 0; y < nativeFont.height; y++, destLine++)
            {
                int destColumnTemp = destColumn;

                for (int x = 0; x < nativeFont.width; x++, destColumnTemp++)
                {
                    int paletteIndex = (charData[(pixelIterated * 2) >> 3] >> ((pixelIterated & 3) * 2)) & 0b11;
                    SColor color = palette[paletteIndex + nativeFont.paletteOffset];

                    usedPalettes.Add(paletteIndex + nativeFont.paletteOffset);

                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4] = (byte)((paletteIndex == 0) ? 0 : 255);
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4 + 1] = (byte)(color.r * 255);
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4 + 2] = (byte)(color.g * 255);
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4 + 3] = (byte)(color.b * 255);

                    pixelIterated++;
                }
            }
        }

        private void UpdateAtlasDataMonochrome(Span<byte> charData, int destCharIndex)
        {
            int destLine = (destCharIndex / CharPerAtlasRow) * nativeFont.height;
            int destColumn = (destCharIndex % CharPerAtlasColumn) * nativeFont.width;

            int pixelIterated = 0;

            for (int y = 0; y < nativeFont.height; y++, destLine++)
            {
                int destColumnTemp = destColumn;

                for (int x = 0; x < nativeFont.width; x++, destColumnTemp++)
                {
                    byte val = ((charData[pixelIterated >> 3] & (1 << (pixelIterated & 7))) != 0) ? (byte)255 : (byte)0;
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4] = val;
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4 + 1] = val;
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4 + 2] = val;
                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4 + 3] = val;

                    pixelIterated++;
                }
            }
        }

        private void UpdateAtlasData2BitPalette(VMMemory memory, byte sourceCharIndex, int destCharIndex)
        {
            int charSize = MemoryUtil.AlignUp(nativeFont.width * nativeFont.height * 2, 8) / 8;

            UpdateAtlasData2BitPalette(nativeFont.fontData[charSize * sourceCharIndex].AsSpan(memory, charSize),
                destCharIndex);
        }

        private void UpdateAtlasDataMonochrome(VMMemory memory, byte sourceCharIndex, int destCharIndex)
        {
            int charSize = MemoryUtil.AlignUp(nativeFont.width * nativeFont.height, 8) / 8;

            UpdateAtlasDataMonochrome(nativeFont.fontData[charSize * sourceCharIndex].AsSpan(memory, charSize),
                destCharIndex);
        }

        private void RefreshAtlasImpl(VMMemory memory)
        {
            usedPalettes.Clear();

            foreach (var charUpdateAndInnerIndex in charIndexInAtlas)
            {
                byte charIndex = nativeFont.charIndexTable[charUpdateAndInnerIndex.Key].Read(memory);
                if (charIndex != 0xFF)
                {
                    largestCharIndex = Math.Max(largestCharIndex, charIndex);

                    if (nativeFont.bpp == 1)
                    {
                        UpdateAtlasDataMonochrome(memory, charIndex, charUpdateAndInnerIndex.Value);
                    }
                    else if (nativeFont.bpp == 2)
                    {
                        UpdateAtlasData2BitPalette(memory, charIndex, charUpdateAndInnerIndex.Value);
                    } else
                    {
                        // Palette font
                        throw new Exception("Unhandled 4-bit palette font!");
                    }
                }
            }
        }

        private ITexture RefreshAtlas(IGraphicDriver driver, VMMemory memory)
        {
            RefreshAtlasImpl(memory);
            ITexture result = driver.CreateTexture(textureData, AtlasWidth, AtlasHeight, 1, TextureFormat.ARGB8888);

            return result;
        }

        private ITexture UpdateAtlas(IGraphicDriver driver, VMMemory memory, string updateCharList)
        {
            foreach (var charToUpdate in updateCharList)
            {
                if (charIndexInAtlas.ContainsKey(charToUpdate))
                {
                    continue;
                }

                byte charIndex = nativeFont.charIndexTable[charToUpdate].Read(memory);
                if (charIndex != 0xFF)
                {
                    largestCharIndex = Math.Max(largestCharIndex, charIndex);

                    if (nativeFont.bpp == 1)
                    {
                        UpdateAtlasDataMonochrome(memory, charIndex, charIndexInAtlas.Count);
                    }
                    else if (nativeFont.bpp == 2)
                    {
                        UpdateAtlasData2BitPalette(memory, charIndex, charIndexInAtlas.Count);
                    } else
                    {
                        // Palette font
                        throw new Exception("Unhandled 4-bit palette font!");
                    }
                }

                charIndexInAtlas.Add(charToUpdate, charIndexInAtlas.Count);
            }

            return driver.CreateTexture(textureData, AtlasWidth, AtlasHeight, 1, TextureFormat.ARGB8888);
        }

        public void DrawText(IGraphicDriver driver, VMMemory memory, int posx, int posy, string text,
            SColor foregroundColor)
        {
            string notInAtlasChar = "";
            foreach (char textChar in text.Distinct())
            {
                if (!charIndexInAtlas.ContainsKey(textChar))
                {
                    notInAtlasChar += textChar;
                }
            }

            ITexture atlas = null;

            if (notInAtlasChar.Length > 0)
            {
                // Purge all existing entries in cache
                atlasCache.Clear();

                RefreshAtlasImpl(memory);
                atlas = UpdateAtlas(driver, memory, notInAtlasChar);

                atlasCache.Add(GetCurrentUseHash(memory), atlas);
            }
            else
            {
                UInt32 hash = GetCurrentUseHash(memory);
                atlas = atlasCache.Get(hash);

                if (atlas == null)
                {
                    atlas = RefreshAtlas(driver, memory);
                    atlasCache.Add(hash, atlas);
                }
            }

            List<int> positions = new();
            foreach (char textChar in text)
            {
                int indexDest = charIndexInAtlas[textChar];

                int line = indexDest / CharPerAtlasRow;
                int column = indexDest % CharPerAtlasColumn;

                positions.Add(column * nativeFont.width);
                positions.Add(line * nativeFont.height);
            }

            driver.DrawText(posx, posy, nativeFont.width, nativeFont.height, positions, atlas,
                TextDirection.Horizontal, (nativeFont.bpp > 1) ? SColor.White : foregroundColor);
        }
    }
}