using Nofun.Driver.Graphics;

namespace Nofun.Module.VMGP
{
    public partial class VMGP
    {
        private SColor[] ScreenPalette = new SColor[256];

        public void InitializePalette()
        {
            for (int i = 0; i < 256; i++)
            {
                ScreenPalette[i] = new SColor(i / 255.0f, i / 255.0f, i / 255.0f);
            }
        }
    }
}