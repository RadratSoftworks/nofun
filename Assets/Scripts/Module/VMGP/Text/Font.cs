using Nofun.Driver.Graphics;
using Nofun.VM;
using Nofun.Util;

using System.Collections.Generic;
using System;
using System.Linq;

namespace Nofun.Module.VMGP
{
    public class Font
    {
        // The maximum amount of characters that a font can hold is 256.
        // So we may spend 16 row of bitmap characters.
        private const int CharPerAtlasRow = 16;
        private const int CharPerAtlasColumn = 16;

        private ITexture atlas;
        private VMGPFont nativeFont;

        private Dictionary<char, int> charIndexInAtlas;
        private byte[] textureData;
        private SColor[] palette;

        private int AtlasWidth => nativeFont.width * CharPerAtlasRow;
        private int AtlasHeight => nativeFont.height * CharPerAtlasColumn;
        private int AtlasByteWidth => AtlasWidth * 4;
        public VMGPFont NativeFont => nativeFont;

        public Font(VMGPFont nativeFont, SColor[] palette)
        {
            if ((nativeFont.bpp != 1) && (nativeFont.bpp != 2))
            {
                throw new InvalidOperationException($"Font with bpp={nativeFont.bpp} is not supported!");
            }

            this.nativeFont = nativeFont;
            this.palette = palette;
            this.charIndexInAtlas = new();

            textureData = new byte[AtlasByteWidth * AtlasHeight];
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

                    textureData[destLine * AtlasByteWidth + destColumnTemp * 4] = (byte)(color.FullBlack ? 0 : 255);
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

        private void UpdateAtlas(IGraphicDriver driver, VMMemory memory, string updateCharList)
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

            if (atlas == null)
            {
                atlas = driver.CreateTexture(textureData, AtlasWidth, AtlasHeight, 1, TextureFormat.ARGB8888);
            }
            else
            {
                atlas.SetData(textureData, 0);
                atlas.Apply();
            }
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

            if (notInAtlasChar.Length > 0)
            {
                UpdateAtlas(driver, memory, notInAtlasChar);
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
                TextDirection.Horizontal, foregroundColor);
        }
    }
}