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
    public unsafe struct MFXMHeader
    {
        public fixed byte magicMhdr[4];
        public ushort totalHeaderSectionSize;
        // XM flag. 0 = Amiga, 1 = Linear
        public ushort flags;
        // The format follows MIDI standard. 0 is single track, 1 is multi track, 2 is multi song.
        public byte format;
        // Channel count from 1-32
        public byte channelCount;
        public byte patternCount;
        public byte sampleCount;
        public byte instrumentCount;
        public byte unk0D;
        public byte defaultSongSpeed;
        public byte defaultBPM;
        public byte unk10;
        public byte songLength;
        // Default row count for each pattern
        // Pattern length is always in 5-bytes unit
        public ushort defaultRowCount;
        public uint unk14;
        public uint unk18;
    }
}