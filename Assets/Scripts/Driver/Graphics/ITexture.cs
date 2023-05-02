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

using System;

namespace Nofun.Driver.Graphics
{
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
        int MipCount { get; }
        TextureFormat Format { get; }

        void SetData(byte[] data, int mipLevel, Memory<SColor> palettes = new Memory<SColor>(), bool zeroAsTransparent = false);
        void Apply();

        /// <summary>
        /// Save texture to a png file.
        /// 
        /// This function main use is for debugging purposes. Implementation may not implement it.
        /// </summary>
        /// <param name="path">Path to save the texture to.</param>
        void SaveToPng(string path);
    }
}