using System;

namespace Nofun.Util
{
    public class MemoryUtil
    {
        public static UInt32 AlignUp(UInt32 value, UInt32 alignAmount)
        {
            return (value + alignAmount - 1) / alignAmount * alignAmount;
        }

        public static Int32 AlignUp(Int32 value, Int32 alignAmount)
        {
            return (value + alignAmount - 1) / alignAmount * alignAmount;
        }
    }
}