using Nofun.Util;

namespace Nofun.Module.VMGP
{
    public struct VMGPRect
    {
        public short x;
        public short y;
        public ushort width;
        public ushort height;

        public NRectangle ToNofunRectangle()
        {
            return new NRectangle()
            {
                x = x,
                y = y,
                width = width,
                height = height
            };
        }
    }
}