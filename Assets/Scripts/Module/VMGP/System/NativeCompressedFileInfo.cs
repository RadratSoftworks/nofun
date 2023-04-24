using System;

namespace Nofun.Module.VMGP
{
    public struct NativeCompressedFileInfo
    {
        public byte cnt;
        public byte offset;
        public ushort crc16;
        public ushort option;
        public uint srcSize;
        public uint destSize;
        public uint literalSize;
    }
}