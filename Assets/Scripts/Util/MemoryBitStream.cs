using System;

namespace Nofun.Util
{
    public class MemoryBitStream : BitStream
    {
        private Memory<byte> memory;

        public override bool Valid => Pointer < memory.Length * 8;

        public MemoryBitStream(Memory<byte> memory)
        {
            this.memory = memory;
        }

        protected override byte GetByte(int index)
        {
            return memory.Span[index];
        }
    }
}