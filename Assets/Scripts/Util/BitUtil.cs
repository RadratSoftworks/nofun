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
    }
}