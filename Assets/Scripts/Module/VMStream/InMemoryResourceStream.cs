using Nofun.Parser;
using System;
using System.IO;

namespace Nofun.Module.VMStream
{
    public class InMemoryResourceStream : IVMHostStream
    {
        private MemoryStream memStream;

        public static IVMHostStream Create(VMGPExecutable executable, uint mode)
        {
            if ((mode & (uint)StreamFlags.Write) != 0)
            {
                throw new InvalidOperationException("Open a resource stream for write is forbidden!");
            }

            uint resourceNumber = (mode >> 16);
            byte[] data = executable.GetResourceData(resourceNumber);

            return new InMemoryResourceStream(data);
        }

        public InMemoryResourceStream(byte[] data)
        {
            memStream = new MemoryStream(data);
        }

        public int Read(Span<byte> buffer, object extraArgs)
        {
            return memStream.Read(buffer);
        }

        public int Ready()
        {
            return (int)StreamFlags.Read;
        }

        public int Seek(int offset, StreamSeekMode whence)
        {
            return (int)memStream.Seek(offset, StreamTranslationUtils.ToSeekOrigin(whence));
        }

        public int Tell()
        {
            return Seek(0, StreamSeekMode.Cur);
        }

        public int Write(Span<byte> buffer, object extraArgs)
        {
            return -1;
        }
    }
}