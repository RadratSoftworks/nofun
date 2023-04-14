using System;

namespace Nofun.Driver.Graphics
{
    public struct SColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SColor(float r, float g, float b, float a = 1.0f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    };
}