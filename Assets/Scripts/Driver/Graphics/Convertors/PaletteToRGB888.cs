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

using System;

namespace Nofun.Driver.Graphics
{
    public static partial class DataConvertor
    {
        public static byte[] PaletteToRGBA8888(Span<byte> data, int width, int height, byte bits, Span<SColor> palettes)
        {
            if ((bits != 1) && (bits != 2) && (bits != 4) && (bits != 8))
            {
                throw new ArgumentException($"Unsupported palette bits {bits}");
            }

            int pixelIterated = 0;
            byte[] result = new byte[width * height * 4];

            int mask = 0xFF >> (8 - bits);
            int iteratedMode = (8 / bits) - 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int paletteIndex = (data[(pixelIterated * bits) >> 3] >> ((pixelIterated & iteratedMode) * bits)) & mask;
                    SColor color = palettes[paletteIndex];

                    result[(y * width + x) * 4] = 255;
                    result[(y * width + x) * 4 + 1] = (byte)(color.r * 255);
                    result[(y * width + x) * 4 + 2] = (byte)(color.g * 255);
                    result[(y * width + x) * 4 + 3] = (byte)(color.b * 255);

                    pixelIterated++;
                }
            }

            return result;
        }
    }
}