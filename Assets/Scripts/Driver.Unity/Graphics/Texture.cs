using Nofun.Driver.Graphics;
using UnityEngine;
using System;
using System.IO;

namespace Nofun.Driver.Unity.Graphics
{
    public class Texture : ITexture
    {
        private Texture2D uTexture;
        private Driver.Graphics.TextureFormat format;
        private int mipCount;

        public Texture2D NativeTexture => uTexture;

        public int Width => uTexture.width;
        public int Height => uTexture.height;
        public int MipCount => mipCount;
        public Driver.Graphics.TextureFormat Format => format;

        private bool DoesFormatNeedTransform(Driver.Graphics.TextureFormat format)
        {
            return (format <= Driver.Graphics.TextureFormat.RGB332) || (format == Driver.Graphics.TextureFormat.RGB555)
                    || (format >= Driver.Graphics.TextureFormat.ARGB1555) || (format == Driver.Graphics.TextureFormat.RGB444);
        }

        private UnityEngine.TextureFormat MapMophunFormatToUnity(Driver.Graphics.TextureFormat format)
        {
            switch (format)
            {
                case Driver.Graphics.TextureFormat.RGB565:
                    return UnityEngine.TextureFormat.RGB565;

                case Driver.Graphics.TextureFormat.RGB888:
                    return UnityEngine.TextureFormat.RGB24;

                case Driver.Graphics.TextureFormat.ARGB4444:
                    return UnityEngine.TextureFormat.ARGB4444;

                case Driver.Graphics.TextureFormat.ARGB8888:
                    return UnityEngine.TextureFormat.ARGB32;

                default:
                    throw new ArgumentException("Unconvertable Mophun format to native Unity format: " + format);
            }
        }

        private void UploadSingleMip(byte[] data, int width, int height, int offset, int sizeInBits, int mipLevel, Span<SColor> palettes)
        {
            bool needTransform = DoesFormatNeedTransform(format);
            int bitsPerPixel = TextureUtil.GetPixelSizeInBits(format);

            // Gurantee that this is in byte-unit.
            if (!needTransform)
            {
                uTexture.SetPixelData(data, mipLevel, offset);
            }
            else
            {
                byte[] finalConverData = null;

                // Transform data into separate buffer, then set pixel data
                Span<byte> dataSpan = new Span<byte>(data, offset, (sizeInBits + 7) >> 3);

                switch (format)
                {
                    case Driver.Graphics.TextureFormat.RGB332:
                        finalConverData = DataConvertor.RGB332ToRGB888(dataSpan, width, height);
                        break;

                    case Driver.Graphics.TextureFormat.Palette2:
                        finalConverData = DataConvertor.PaletteToRGBA8888(dataSpan, width, height, 1, palettes);
                        break;

                    case Driver.Graphics.TextureFormat.Palette4:
                        finalConverData = DataConvertor.PaletteToRGBA8888(dataSpan, width, height, 2, palettes);
                        break;

                    case Driver.Graphics.TextureFormat.Palette16:
                    case Driver.Graphics.TextureFormat.Palette16_Alt:
                        finalConverData = DataConvertor.PaletteToRGBA8888(dataSpan, width, height, 4, palettes);
                        break;

                    case Driver.Graphics.TextureFormat.Palette256:
                    case Driver.Graphics.TextureFormat.Palette256_Alt:
                        finalConverData = DataConvertor.PaletteToRGBA8888(dataSpan, width, height, 8, palettes);
                        break;

                    default:
                        break;
                }

                if (finalConverData == null)
                {
                    throw new Exception("Format that need to be converted is unhandled!");
                }
                else
                {
                    uTexture.SetPixelData(finalConverData, mipLevel);
                }
            }
        }

        private void UploadData(byte[] data, int width, int height, int mipCount, Span<SColor> palettes)
        {
            bool needTransform = DoesFormatNeedTransform(format);
            int bitsPerPixel = TextureUtil.GetPixelSizeInBits(format);

            // These format normally also need individual conversion
            int dataPointerInBits = 0;

            for (int i = 0; (i < mipCount) && (width !=0) && (height != 0); i++)
            {
                int dataSizeInBits = width * height * bitsPerPixel;
                UploadSingleMip(data, width, height, dataPointerInBits >> 3, dataSizeInBits, i, palettes);

                // All start in byte boundary
                dataPointerInBits += (dataSizeInBits + 7) / 8 * 8;

                width >>= 2;
                height >>= 2;
            }

            uTexture.Apply();
        }

        public Texture(byte[] data, int width, int height, int mipCount, Driver.Graphics.TextureFormat format, Span<SColor> palettes)
        {
            bool needTransform = DoesFormatNeedTransform(format);

            this.format = format;
            this.mipCount = mipCount;

            uTexture = new Texture2D(width, height, needTransform ? UnityEngine.TextureFormat.RGB24 : MapMophunFormatToUnity(format), true);
            UploadData(data, width, height, mipCount, palettes);
        }

        public void SetData(byte[] data, int mipLevel, Span<SColor> palettes)
        {
            UploadSingleMip(data, uTexture.width >> mipLevel, uTexture.height >> mipLevel, 0, data.Length * 8, mipLevel, palettes);
        }

        public void Apply()
        {
            uTexture.Apply();
        }

        public void SaveToPng(string path)
        {
            using (FileStream stream = File.OpenWrite(path))
            {
                byte[] pngData = uTexture.EncodeToPNG();
                stream.Write(pngData, 0, pngData.Length);
            }
        }
    }
}