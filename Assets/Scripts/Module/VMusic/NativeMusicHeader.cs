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

 namespace Nofun.Module.VMusic
 {
    public unsafe struct NativeMusicHeader
    {
        public fixed byte magicMhdr[4];
        public ushort totalDataSize;
        public ushort unk06_Flags;
        public byte unk08;
        public byte unk09;
        public byte unkWordCount;
        public byte unk0B;
        public byte unk0C;
        public byte unk0D;
        public byte unk0E;
        public byte sampleBlockSize;
        public byte unk10;
        public byte unkDataSize;
        public ushort wordFillForUnkWord;
        public uint unk14;
        public uint unk18;
    }
 }