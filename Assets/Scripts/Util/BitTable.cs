using System;

namespace Nofun.Util
{
    public class BitTable
    {
        private uint[] values;
        private uint maxCount;

        public BitTable(uint maxCount)
        {
            this.maxCount = maxCount;
            values = new uint[((this.maxCount + 31) >> 5)];

            Array.Fill(values, 0xFFFFFFFF);
        }

        public int Allocate()
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == 0)
                {
                    continue;
                }

                if (values[i] == 0xFFFFFFFF)
                {
                    values[i] &= ~1u;
                    return i * 32;
                }

                // There is no BitOperations in Unity atm
                // So we will manually search
                for (int j = 0; j < 32; j++)
                {
                    if ((values[i] & (uint)(1 << j)) != 0)
                    {
                        values[i] &= ~(uint)(1 << j);
                        return i * 32 + j;
                    }
                }
            }

            return -1;
        }

        public void Set(uint offset)
        {
            values[offset >> 5] &= ~(uint)(1 << (int)(offset & 31));
        }

        public void Clear(uint offset)
        {
            values[offset >> 5] |= (uint)(1 << (int)(offset & 31)); 
        }
    }
}