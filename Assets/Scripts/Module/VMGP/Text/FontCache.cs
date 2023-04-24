using Nofun.Driver.Graphics;
using System.Collections.Generic;

namespace Nofun.Module.VMGP
{
    public class FontCache
    {
        private Dictionary<uint, Font> fonts = new();

        public Font Retrieve(VMGPFont nativeFont, SColor[] palette)
        {
            uint dataAddr = nativeFont.fontData.Value;

            if (fonts.ContainsKey(dataAddr))
            {
                Font result = fonts[dataAddr];
                if ((result.NativeFont.width != nativeFont.width) ||
                    (result.NativeFont.height != nativeFont.height) ||
                    (result.NativeFont.charIndexTable != nativeFont.charIndexTable))
                {
                    // Will create a new font entry and replace this existing one
                    fonts.Remove(dataAddr);
                }
                else
                {
                    return result;
                }
            }

            Font newFont = new Font(nativeFont, palette);
            fonts.Add(dataAddr, newFont);

            return newFont;
        }
    }
}