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