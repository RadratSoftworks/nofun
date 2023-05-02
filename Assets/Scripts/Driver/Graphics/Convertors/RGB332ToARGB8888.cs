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
        private static readonly byte[] TwoBitsRGB332Palette =
        {
            0, 85, 170, 255
        };

        private static readonly byte[] ThreeBitsRGB332Palette =
        {
            0, 36, 73, 109, 146, 182, 219, 255
        };

        public static byte[] RGB332ToARGB8888(Span<byte> data, int width, int height, bool zeroAsTransparent)
        {
            byte[] newData = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixel = data[y * width + x];
                    if (zeroAsTransparent && (pixel == 0))
                    {
                        newData[y * width * 4 + x * 4] = 0;
                        newData[y * width * 4 + x * 4 + 1] = 0;
                        newData[y * width * 4 + x * 4 + 2] = 0;
                        newData[y * width * 4 + x * 4 + 3] = 0;
                    }
                    else
                    {
                        newData[y * width * 4 + x * 4] = 255;
                        newData[y * width * 4 + x * 4 + 1] = ThreeBitsRGB332Palette[(pixel >> 5) & 0b111];
                        newData[y * width * 4 + x * 4 + 2] = ThreeBitsRGB332Palette[(pixel >> 2) & 0b111];
                        newData[y * width * 4 + x * 4 + 3] = TwoBitsRGB332Palette[pixel & 0b11];
                    }
                }
            }

            return newData;
        }
    }
}