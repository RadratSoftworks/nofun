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

        private uint mode;

        public static IVMHostStream Create(VMGPExecutable executable, uint mode)
        {
            return new VMResourceStream(executable, (mode >> 16), mode & 0xFFFF);
        }

        public VMResourceStream(VMGPExecutable executable, uint resourceNumber, uint mode)
        {
            this.executable = executable;
            this.mode = mode;
            this.resourceNumber = resourceNumber;
            this.maxSize = executable.GetResourceSize(resourceNumber);
        }

        public int Read(Span<byte> buffer, object extraArgs)
        {
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
            return (int)currentPointer;
        }

        public int Write(Span<byte> buffer, object extraArgs)
        {
            if (!BitUtil.FlagSet(mode, StreamFlags.Write))
            {
                throw new ArgumentException("The stream is not writable!");
            }

            uint countWrite = Math.Min((uint)buffer.Length, maxSize - currentPointer);
            if (countWrite != buffer.Length)
            {
                buffer = buffer.Slice(0, (int)countWrite);
            }

            executable.WriteResourceData(resourceNumber, buffer, currentPointer);
            currentPointer += countWrite;

            return (int)countWrite;
        }
    }
}