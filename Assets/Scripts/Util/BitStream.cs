using System;

namespace Nofun.Util
{
    public abstract class BitStream
    {
        private int pointer;

        protected int Pointer => pointer;

        public abstract bool Valid { get; }

        protected abstract byte GetByte(int index);

        public void Forward(int offset)
        {
            pointer += offset;
        }

        public ulong ReadBits(int count)
        {
            if (count > 64)
            {
                throw new ArgumentException("Can't read more than 64 bits at once!");
            }

            ulong result = 0;
            int bitLeft = count;
            int bitRead = 0;

            if (pointer % 8 != 0)
            {
                int toExtract = Math.Min(8 - pointer % 8, bitLeft);

                result = ((ulong)GetByte(pointer >> 3) >> (8 - ((pointer % 8) + toExtract))) & (ulong)(0xFF >> (8 - toExtract));

                bitLeft -= toExtract;

                bitRead += toExtract;
                pointer += toExtract;
            }

            if (bitLeft == 0)
            {
                return result;
            }

            for (int i = 0; i < (bitLeft >> 3); i++)
            {
                byte rawByte = GetByte(pointer >> 3);

                result <<= 8;
                result |= (ulong)rawByte;

                bitLeft -= 8;

                bitRead += 8;
                pointer += 8;
            }

            if (bitLeft == 0)
            {
                return result;
            }

            // Read the remaining. From here the pointer should be aligned already
            // Note to self: Since bitLeft is less then 8, we don't have to worry about the data shifting to another word
            result <<= bitLeft;
            result |= (ulong)GetByte(pointer >> 3) >> (8 - bitLeft);
            pointer += bitLeft;

            return result;
        }
    };
}