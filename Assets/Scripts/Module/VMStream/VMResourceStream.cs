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

using Nofun.Parser;
using Nofun.Util;
using System;
using System.IO;

namespace Nofun.Module.VMStream
{
    public class VMResourceStream : IVMHostStream
    {
        private uint currentPointer = 0;
        private uint maxSize = 0;

        private uint resourceNumber = 0;
        private VMGPExecutable executable;
        private FileStream groundStream;

        private uint mode;
        private string filePath;

        public static IVMHostStream Create(VMGPExecutable executable, string basePath, uint mode)
        {
            return new VMResourceStream(executable, basePath, (mode >> 16), mode & 0xFFFF);
        }

        public VMResourceStream(VMGPExecutable executable, string baseStorePath, uint resourceNumber, uint mode)
        {
            string baseResourcePath = Path.Join(baseStorePath, "__Resources");
            Directory.CreateDirectory(baseResourcePath);

            this.filePath = Path.Join(baseResourcePath, $"{resourceNumber:X8}");

            try
            {
                groundStream = File.Open(this.filePath, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (Exception _)
            {
                // Nothing yet
            }

            this.executable = executable;
            this.mode = mode;
            this.resourceNumber = resourceNumber;
            this.maxSize = executable.GetResourceSize(resourceNumber);
        }

        public int Read(Span<byte> buffer, object extraArgs)
        {
            if (groundStream != null)
            {
                return groundStream.Read(buffer);
            }

            uint countRead = Math.Min((uint)buffer.Length, maxSize - currentPointer);
            if (countRead != buffer.Length)
            {
                buffer = buffer.Slice(0, (int)countRead);
            }

            executable.ReadResourceData(resourceNumber, buffer, currentPointer);
            currentPointer += countRead;

            return (int)countRead;
        }

        public int Ready()
        {
            return (int)mode;
        }

        public int Seek(int offset, StreamSeekMode whence)
        {
            if (groundStream != null)
            {
                return (int)groundStream.Seek(offset, whence.ToSeekOrigin());
            }

            switch (whence)
            {
                case StreamSeekMode.Cur:
                    currentPointer = (uint)(Math.Max(0, currentPointer + offset));
                    break;

                case StreamSeekMode.Set:
                    currentPointer = (uint)Math.Max(0, offset);
                    break;

                case StreamSeekMode.End:
                    currentPointer = (uint)(Math.Max(0, maxSize + offset));
                    break;

                default:
                    throw new ArgumentException($"Unknown whence mode {whence}");
            }

            currentPointer = Math.Min(maxSize, currentPointer);
            return (int)currentPointer;
        }

        public int Tell()
        {
            return (int?)groundStream?.Position ?? (int)currentPointer;
        }

        public int Write(Span<byte> buffer, object extraArgs)
        {
            if (!BitUtil.FlagSet(mode, StreamFlags.Write))
            {
                throw new ArgumentException("The stream is not writable!");
            }

            if (groundStream == null)
            {
                byte[] wholeData = new byte[maxSize];
                executable.ReadResourceData(resourceNumber, wholeData, 0);

                // Create file and copy new data to it
                groundStream = File.Open(this.filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                groundStream.Write(wholeData);
                groundStream.Seek(currentPointer, SeekOrigin.Begin);
            }

            int maxWrite = Math.Clamp(buffer.Length, 0, (int)(maxSize - groundStream.Position));
            groundStream.Write(buffer.Slice(0, maxWrite));

            return maxWrite;
        }

        public void OnClose()
        {
            groundStream?.Close();
        }
    }
}