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
        public short xPan;
        public short yPan;
        public short xPos;
        public short yPos;
        public VMPtr<byte> mapData;
        public VMPtr<byte> tileSpriteData;
    };
}