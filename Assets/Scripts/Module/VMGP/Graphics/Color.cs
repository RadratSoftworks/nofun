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
                return SColor.FromRgb555(rgb555);
            }

            return ScreenPalette[Math.Clamp(colorOrPalIndex, 0, 255)];
        }
    }
}