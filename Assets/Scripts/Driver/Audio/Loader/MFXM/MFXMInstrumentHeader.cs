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

namespace Nofun.Loader.MFXM
{
    public struct MFXMInstrumentHeader
    {
        public ushort headerSize;
        /// <summary>
        /// Bit 0: On - contains volume points
        /// Bit 1: On - contains panning points
        /// Bit 2: On - Contains vibrato data
        /// </summary>
        public byte flag;
        public byte sampleNumbersCount;
        public ushort volumeFade;
    }
}