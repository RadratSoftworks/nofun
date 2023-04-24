/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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