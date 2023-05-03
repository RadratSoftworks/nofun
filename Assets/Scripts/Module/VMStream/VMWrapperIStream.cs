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
using Nofun.Util.Logging;
using System;
using System.IO;

namespace Nofun.Module.VMGP
{
    public class VMWrapperIStream : IVMHostStream
    {
        private Stream stream;

        public static IVMHostStream Create(string persistentFolderPath, string fileToOpen, uint mode)
        {
            string filePath = Path.Join(persistentFolderPath, fileToOpen);

            FileAccess access = FileAccess.Read;
            bool readable = BitUtil.FlagSet(mode, StreamFlags.Read);
            bool writable = BitUtil.FlagSet(mode, StreamFlags.Write);

            if (readable && writable)
            {
                access = FileAccess.ReadWrite;
            }
            else if (writable)
            {
                access = FileAccess.Write;
            }

            FileMode openMode = writable ? FileMode.OpenOrCreate : FileMode.Open;

            // Truncate and create is very similar, except that truncate requires the file to already exist
            // If both these flags present, prefer create
            if (BitUtil.FlagSet(mode, StreamFlags.Trunc))
            {
                openMode = FileMode.Truncate;
            }
            if (BitUtil.FlagSet(mode, StreamFlags.Create))
            {
                openMode = FileMode.Create;
            }
            if (BitUtil.FlagSet(mode, StreamFlags.MustNotExistBefore) && ((openMode == FileMode.Create) || (openMode == FileMode.Truncate)))
            {
                if (File.Exists(filePath))
                {
                    throw new Exception($"File with path: {fileToOpen} already existed while being requested to open in EXCL mode!");
                }
            }

            return new VMWrapperIStream(new FileStream(filePath, openMode, access));
        }

        public VMWrapperIStream(Stream baseStream)
        {
            this.stream = baseStream;
        }

        public int Read(Span<byte> buffer, object extraArgs)
        {
            try
            {
                return stream.Read(buffer);
            }
            catch (Exception e)
            {
                Logger.Trace(LogClass.VMStream, $"Reading stream failed with error: {e}");
                return -1;
            }
        }

        public int Ready()
        {
            int flags = 0;

            if (stream.CanRead)
            {
                flags |= (int)StreamFlags.Read;
            }

            if (stream.CanWrite)
            {
                flags |= (int)StreamFlags.Read;
            }

            return flags;
        }

        public int Seek(int offset, StreamSeekMode whence)
        {
            try
            {
                return (int)stream.Seek(offset, StreamTranslationUtils.ToSeekOrigin(whence));
            }
            catch (Exception e)
            {
                Logger.Trace(LogClass.VMStream, $"Seeking stream failed with error: {e}");
                return -1;
            }
        }

        public int Tell()
        {
            return (int)stream.Seek(0, SeekOrigin.Current);
        }

        public int Write(Span<byte> buffer, object extraArgs)
        {
            try
            {
                long previousOffset = stream.Seek(0, SeekOrigin.Current);
                stream.Write(buffer);
                long currentOffset = stream.Seek(0, SeekOrigin.Current);

                return (int)(currentOffset - previousOffset);
            }
            catch (Exception e)
            {
                Logger.Trace(LogClass.VMStream, $"Writing stream failed with error: {e}");
                return -1;
            }
        }
    }
}