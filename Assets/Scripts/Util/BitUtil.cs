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
    }
}