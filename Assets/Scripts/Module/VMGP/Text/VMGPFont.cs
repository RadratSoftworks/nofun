using Nofun.VM;

namespace Nofun.Module.VMGP
{
    public struct VMGPFont
    {
        public VMPtr<byte> fontData;
        public VMPtr<byte> charIndexTable;
        public byte bpp;
        public byte width;
        public byte height;
        public byte paletteOffset;
    };
}