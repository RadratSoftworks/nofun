using Nofun.Driver.Graphics;
using System;

namespace Nofun.Module.VMGP
{
    public partial class VMGP
    {
        public SColor GetColor(Int32 colorOrPalIndex)
        {
            // Bit 31 set, accidentally be a color
            if (colorOrPalIndex < 0)
            {
                ushort rgb555 = (ushort)(colorOrPalIndex & 0xFFFF);
                return new SColor((rgb555 & 0b11111) / 31.0f,
                    ((rgb555 >> 5) & 0b11111) / 31.0f,
                    ((rgb555 >> 10) & 0b11111) / 31.0f);
            }

            return ScreenPalette[Math.Clamp(colorOrPalIndex, 0, 255)];
        }
    }
}