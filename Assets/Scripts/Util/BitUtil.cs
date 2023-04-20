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