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
    public class BitTable
    {
        private uint[] values;
        private uint maxCount;

        public uint Capacity => maxCount;

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