using System;
using NoAlloq;
using Nofun.Driver.Graphics;
using Nofun.Util.Logging;
using Nofun.VM;

namespace Nofun.Module.VMGP3D
{
    [Module]
    public partial class VMGP3D
    {
        private SimpleObjectManager<ITexture> managedTextures;

        internal static void ResolveMophunSWTransparency(byte[] data, int textureWidth, int textureHeight, int mipCount, TextureFormat format)
        {
            if ((format != Driver.Graphics.TextureFormat.ARGB4444) && (format != Driver.Graphics.TextureFormat.ARGB8888))
            {
                return;
            }

            Span<byte> dataSpan = data;
            byte bitsSize = (byte)TextureUtil.GetPixelSizeInBits(format);

            // According to docs, if the top bit on, it's transparent, lol
            // At least, that's what it's with software rendering, which is used by almost all games...
            while ((mipCount-- > 0) && (textureWidth > 0) && (textureHeight > 0))
            {
                switch (format)
                {
                    case Driver.Graphics.TextureFormat.ARGB4444:
                        {
                            for (int y = 0; y < textureHeight; y++)
                            {
                                for (int x = 0; x < textureWidth; x++)
                                {
                                    byte ar = dataSpan[y * textureWidth * 2 + x * 2 + 1];

                                    if ((ar & (1 << 7)) == 0)
                                    {
                                        ar |= 0xF0;
                                    }
                                    else
                                    {
                                        ar = (byte)(ar & ~0xF0);
                                    }

                                    dataSpan[y * textureWidth * 2 + x * 2 + 1] = ar;
                                }
                            }

                            break;
                        }

                    case Driver.Graphics.TextureFormat.ARGB8888:
                        {
                            for (int y = 0; y < textureHeight; y++)
                            {
                                for (int x = 0; x < textureWidth; x++)
                                {
                                    byte a = dataSpan[y * textureWidth * 4 + x * 4 + 3];

                                    if ((a & (1 << 7)) != 0)
                                    {
                                        dataSpan[y * textureWidth * 4 + x * 4 + 3] = 0;
                                    }
                                    else
                                    {
                                        dataSpan[y * textureWidth * 4 + x * 4 + 3] = 255;
                                    }
                                }
                            }

                            break;
                        }
                }

                textureWidth >>= 1;
                textureHeight >>= 1;

                dataSpan = dataSpan.Slice(textureWidth * textureHeight * bitsSize / 8);
            }
        }

        [ModuleCall]
        private int vCreateTexture(VMPtr<NativeTexture> tex)
        {
            NativeTexture texValue = tex.Read(system.Memory);

            int textureWidth = 1 << (int)texValue.lodX;
            int textureHeight = 1 << (int)texValue.lodY;
            TextureFormat format = (TextureFormat)texValue.textureFormat;

            long texSize = TextureUtil.GetTextureSizeInBytes(textureWidth, textureHeight, format);
            SColor[] palettes = null;
            bool zeroAsTrans = true;

            Span<byte> dataSpan = texValue.textureData.AsSpan(system.Memory, (int)texSize);
            byte[] data = dataSpan.ToArray();

            if (TextureUtil.IsPaletteFormat(format))
            {
                if (TextureUtil.IsPaletteSelfProvidedInTexture(format))
                {
                    bool argb = TextureUtil.IsPaletteARGB8(format);

                    palettes = texValue.palette.AsSpan(system.Memory, (int)texValue.paletteCount).Select(
                        color => argb ? SColor.FromArgb8888SW(color) : SColor.FromRgb888(color)).ToArray();
                    zeroAsTrans = false;
                }
                else
                {
                    palettes = system.VMGPModule.ScreenPalette;
                }
            }
            else
            {
                ResolveMophunSWTransparency(data, textureWidth, textureHeight, (int)texValue.mipmapCount + 1, format);
            }

            ITexture texDriver = system.GraphicDriver.CreateTexture(data, textureWidth, textureHeight,
                (int)texValue.mipmapCount + 1, format, palettes, zeroAsTrans);

            return managedTextures.Add(texDriver);
        }

        [ModuleCall]
        private void vDeleteTexture(int handle)
        {
            managedTextures.Remove(handle);
        }

        [ModuleCall]
        private void vSetActiveTexture(int index)
        {
            ITexture refer = managedTextures.Get(index);
            if (refer == null)
            {
                Logger.Error(LogClass.VMGP3D, $"Setting a null texture with handle {index}");
            }
            else
            {
                activeTexture = refer;
                system.GraphicDriver.MainTexture = activeTexture;
            }
        }
    }
}