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

using Nofun.VM;

namespace Nofun.Module.VMGP
{
    public struct VMGPFont
    {
        public VMPtr<byte> fontData;
        public VMPtr<byte> charIndexTable;
        public byte bpp;
        public byte width;
        public byte height;
        public byte paletteOffset;
    };
}