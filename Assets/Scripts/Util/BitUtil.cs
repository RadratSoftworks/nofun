using System;

namespace Nofun.Util
{
    public static class BitUtil
    {
        public static UInt32 SignExtend(UInt16 value)
        {
            return (value & 0x7FFFu) | ((value & 0x8000u) << 16);
        }
    }
}