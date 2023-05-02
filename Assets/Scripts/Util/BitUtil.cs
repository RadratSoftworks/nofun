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

namespace Nofun.Util
{
    public static class BitUtil
    {
        // https://stackoverflow.com/questions/31374628/fast-way-of-finding-most-and-least-significant-bit-set-in-a-64-bit-integer
        // Thanks Mr Andrew!
        private const ulong Magic = 0x37E84A99DAE458F;

        private static readonly int[] MagicTable =
        {
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        };

        public static uint SignExtend(ushort value)
        {
            return (uint)((short)value);
        }

        public static uint SignExtend(byte value)
        {
            return (uint)((sbyte)value);
        }

        public static ushort SignExtendToShort(byte value)
        {
            return (ushort)((sbyte)value);
        }

        public static bool FlagSet<T>(uint value, T flag) where T: Enum
        {
            return ((value & (uint)(object)flag) != 0);
        }

        public static uint BytesToUint(byte[] bytes)
        {
            switch (bytes.Length)
            {
                case 1:
                    return bytes[0];

                case 2:
                    return BitConverter.ToUInt16(bytes);

                case 3:
                    return bytes[0] | ((uint)bytes[1] << 8) | ((uint)bytes[2] << 16);

                case 4:
                    return BitConverter.ToUInt32(bytes);

                default:
                    throw new ArgumentException($"Can't convert byte array with length of {bytes.Length} to uint!");
            }
        }

        public static int BitScanForward(ulong b)
        {
            return MagicTable[((ulong)((long)b & -(long)b) * Magic) >> 58];
        }

        public static int BitScanReverse(ulong b)
        {
            b |= b >> 1;
            b |= b >> 2;
            b |= b >> 4;
            b |= b >> 8;
            b |= b >> 16;
            b |= b >> 32;
            b = b & ~(b >> 1);
            return MagicTable[b * Magic >> 58];
        }
    }
}