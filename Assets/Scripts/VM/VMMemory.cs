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
using System.Buffers.Binary;

namespace Nofun.VM
{
    public class VMMemory
    {
        public const uint DataAlignment = 0x1000;

        public byte[] memory;

        public VMMemory(uint memorySize)
        {
            memory = new byte[memorySize];
        }

        public Span<byte> GetMemorySpan(int offset, int size)
        {
            if ((offset >= 0) && (offset < DataAlignment))
            {
                return new Span<byte>();
            }

            return new Span<byte>(memory, offset, size);
        }

        public Memory<byte> GetMemoryMemory(int offset, int size)
        {
            if ((offset >= 0) && (offset < DataAlignment))
            {
                return new Memory<byte>();
            }

            return new Memory<byte>(memory, offset, size);
        }

        #region Memory Access
        public UInt32 ReadMemory32(UInt32 address)
        {
            return BitConverter.ToUInt32(memory, (int)address);
        }

        public UInt16 ReadMemory16(UInt32 address)
        {
            return BitConverter.ToUInt16(memory, (int)address);
        }

        public byte ReadMemory8(UInt32 address)
        {
            return memory[address];
        }

        public void WriteMemory32(UInt32 address, UInt32 value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(new Span<byte>(memory, (int)address, 4), value);
        }

        public void WriteMemory16(UInt32 address, UInt16 value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(new Span<byte>(memory, (int)address, 2), value);
        }

        public void WriteMemory8(UInt32 address, byte value)
        {
            memory[address] = value;
        }

        public void MemoryCopy(UInt32 destAddr, UInt32 sourceAddr, UInt32 count)
        {
            Buffer.BlockCopy(memory, (int)sourceAddr, memory, (int)destAddr, (int)count);
        }

        public void MemorySet(UInt32 destAddr, byte fillByte, UInt32 count)
        {
            Array.Fill(memory, fillByte, (int)destAddr, (int)count);
        }
        #endregion
    }
}