using System;

namespace Nofun.Module.VMStream
{
    public interface IVMHostStream
    {
        int Read(Span<byte> buffer, object extraArgs);

        int Write(Span<byte> buffer, object extraArgs);

        int Seek(int offset, StreamSeekMode whence);

        int Tell();

        int Ready();
    };
}