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
            return new Span<byte>(memory, offset, size);
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
        #endregion
    }
}