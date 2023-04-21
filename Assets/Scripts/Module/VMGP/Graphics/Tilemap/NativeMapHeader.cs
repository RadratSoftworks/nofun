using Nofun.VM;

namespace Nofun.Module.VMGP
{
    public struct NativeMapHeader
    {
        public byte flag;
        public byte format;
        public byte mapWidth;
        public byte mapHeight;
        public byte animationSpeed;
        public byte animationCount;
        public byte animationActive;
        public byte pad;
        public ushort xPan;
        public ushort yPan;
        public ushort xPos;
        public ushort yPos;
        public VMPtr<byte> mapData;
        public VMPtr<byte> tileSpriteData;
    };
}