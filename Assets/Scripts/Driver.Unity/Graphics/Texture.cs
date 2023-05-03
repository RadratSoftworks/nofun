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
        private MpFilterMode cachedFilter;
        private UnityEngine.FilterMode cachedUnityFilter;
        private Vector2 cachedSize;

        public Texture2D NativeTexture => uTexture;

        public int Width => (int)cachedSize.x;
        public int Height => (int)cachedSize.y;
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

        private void UploadSingleMip(byte[] data, int width, int height, int offset, int sizeInBits, int mipLevel, Memory<SColor> palettes, bool zeroAsTransparent)
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
                        finalConverData = DataConvertor.RGB332ToARGB8888(dataSpan, width, height, zeroAsTransparent);
                        break;

                    case Driver.Graphics.TextureFormat.Palette2:
                        finalConverData = DataConvertor.PaletteToARGB8888(dataSpan, width, height, 1, palettes, zeroAsTransparent);
                        break;

                    case Driver.Graphics.TextureFormat.Palette4:
                        finalConverData = DataConvertor.PaletteToARGB8888(dataSpan, width, height, 2, palettes, zeroAsTransparent);
                        break;

                    case Driver.Graphics.TextureFormat.Palette16:
                    case Driver.Graphics.TextureFormat.Palette16_Alt:
                        finalConverData = DataConvertor.PaletteToARGB8888(dataSpan, width, height, 4, palettes, zeroAsTransparent);
                        break;

                    case Driver.Graphics.TextureFormat.Palette256:
                    case Driver.Graphics.TextureFormat.Palette256_Alt:
                        finalConverData = DataConvertor.PaletteToARGB8888(dataSpan, width, height, 8, palettes, zeroAsTransparent);
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

        private void UploadData(byte[] data, int width, int height, int mipCount, Memory<SColor> palettes, bool zeroAsTransparent)
        {
            bool needTransform = DoesFormatNeedTransform(format);
            int bitsPerPixel = TextureUtil.GetPixelSizeInBits(format);

            // These format normally also need individual conversion
            int dataPointerInBits = 0;

            for (int i = 0; (i < mipCount) && (width != 0) && (height != 0); i++)
            {
                int dataSizeInBits = width * height * bitsPerPixel;
                UploadSingleMip(data, width, height, dataPointerInBits >> 3, dataSizeInBits, i, palettes, zeroAsTransparent);

                // All start in byte boundary
                dataPointerInBits += (dataSizeInBits + 7) / 8 * 8;

                width >>= 1;
                height >>= 1;
            }

            uTexture.Apply();
        }

        public Texture(byte[] data, int width, int height, int mipCount, Driver.Graphics.TextureFormat format, Memory<SColor> palettes, bool zeroAsTransparent = false)
        {
            bool needTransform = DoesFormatNeedTransform(format);

            this.format = format;
            this.mipCount = mipCount;
            this.cachedSize = new Vector2(width, height);

            uTexture = new Texture2D(width, height, needTransform ? UnityEngine.TextureFormat.ARGB32 : MapMophunFormatToUnity(format), true);
            uTexture.filterMode = FilterMode.Trilinear;

            UploadData(data, width, height, mipCount, palettes, zeroAsTransparent);

            cachedUnityFilter = FilterMode.Trilinear;
            cachedFilter = MpFilterMode.MipLinear;
        }

        public void SetData(byte[] data, int mipLevel, Memory<SColor> palettes, bool zeroAsTransparent = false)
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                UploadSingleMip(data, uTexture.width >> mipLevel, uTexture.height >> mipLevel, 0, data.Length * 8, mipLevel, palettes, zeroAsTransparent);
            });
        }

        public void Apply()
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                uTexture.Apply();
            });
        }

        public void SaveToPng(string path)
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                using (FileStream stream = File.OpenWrite(path))
                {
                    byte[] pngData = uTexture.EncodeToPNG();
                    stream.Write(pngData, 0, pngData.Length);
                }
            });
        }

        public MpFilterMode Filter
        {
            get => cachedFilter;
            set
            {
                cachedFilter = value;

                FilterMode unityFilter = value.ToUnity();
                if (cachedUnityFilter != unityFilter)
                {
                    cachedUnityFilter = unityFilter;
                    JobScheduler.Instance.RunOnUnityThread(() =>
                    {
                        uTexture.filterMode = unityFilter;
                    });
                }
            }
        }
    }
}