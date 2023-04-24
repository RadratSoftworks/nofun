using Nofun.Module.VMStream;
using Nofun.Util;
using System;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMGP
{
    public class VMStreamBitStream : BitStream
    {
        private IVMHostStream stream;
        private long streamSize;
        private long currentBytePos;
        private byte currentByte;
        private int originOffset;

        public VMStreamBitStream(IVMHostStream stream)
        {
            originOffset = stream.Tell();

            streamSize = stream.Seek(0, StreamSeekMode.End) - originOffset;
            stream.Seek(originOffset, StreamSeekMode.Set);

            currentBytePos = -100;
            currentByte = 0;

            this.stream = stream;
        }

        public override bool Valid => Pointer < streamSize * 8;

        protected override byte GetByte(int index)
        {
            if (currentBytePos != index)
            {
                Span<byte> readByte = MemoryMarshal.CreateSpan(ref currentByte, 1);

                if ((index - currentBytePos) != 1)
                {
                    stream.Seek(originOffset + index, StreamSeekMode.Set);
                }

                stream.Read(readByte, null);
                currentBytePos = index;
            }

            return currentByte;
        }
    }
}