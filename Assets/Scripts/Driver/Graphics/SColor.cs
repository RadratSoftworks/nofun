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

        public float Difference(SColor another)
        {
            return Math.Abs(r - another.r) + Math.Abs(g - another.g) + Math.Abs(b - another.b);
        }

        public uint ToRgb555()
        {
            return (uint)(b * 31) | ((uint)(g * 31) << 5) | ((uint)(r * 31) << 10);
        }

        public static SColor FromRgb555(uint value)
        {
            return new SColor(((value >> 10) & 0b11111) / 31.0f,
                    ((value >> 5) & 0b11111) / 31.0f,
                    (value & 0b11111) / 31.0f);
        }
    };
}