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
using System.IO;

using Nofun.Util;

namespace Nofun.Module.VMStream
{
    public class StandardStreamVM : Stream
    {
        private IVMHostStream originalStream;

        public StandardStreamVM(IVMHostStream behind)
        {
            this.originalStream = behind;
        }

        public override bool CanRead => (BitUtil.FlagSet((uint)originalStream.Ready(), StreamFlags.Read));

        public override bool CanSeek => true;

        public override bool CanWrite => (BitUtil.FlagSet((uint)originalStream.Ready(), StreamFlags.Write));

        public override long Length
        {
            get
            {
                int current = originalStream.Seek(0, StreamSeekMode.Cur);
                int length = originalStream.Seek(0, StreamSeekMode.End);
                originalStream.Seek(current, StreamSeekMode.Set);

                return length;
            }
        }

        public override long Position
        {
            get
            {
                return originalStream.Seek(0, StreamSeekMode.Cur);
            }
            set
            {
                originalStream.Seek((int)value, StreamSeekMode.Set);
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return originalStream.Read(buffer.AsSpan(offset, count), null);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return originalStream.Seek((int)offset, origin.ToSeekMode());
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            originalStream.Write(buffer.AsSpan(offset, count), null);
        }
    }
}