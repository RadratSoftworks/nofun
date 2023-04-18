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

        public static byte[] RGB332ToRGB888(Span<byte> data, int width, int height)
        {
            byte[] newData = new byte[width * height * 3];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixel = data[y * width + x];

                    newData[y * width * 3 + x * 3 + 0] = ThreeBitsRGB332Palette[(pixel >> 5) & 0b111];
                    newData[y * width * 3 + x * 3 + 1] = ThreeBitsRGB332Palette[(pixel >> 2) & 0b111];
                    newData[y * width * 3 + x * 3 + 2] = TwoBitsRGB332Palette[pixel & 0b11];
                }
            }

            return newData;
        }
    }
}