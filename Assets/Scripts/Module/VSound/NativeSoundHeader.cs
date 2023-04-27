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

namespace Nofun.Module.VSound
{
    public unsafe struct NativeSoundHeader
    {
        public fixed byte magicShdr[4];
        public uint headerSize;
        public uint format;
        public uint loopStartFrame;
        public uint loopEndFrame;
        public uint frequency;
        public byte bitsPerSample;
        public byte channelCount;
        public byte priority;
        public byte pad;
        public fixed byte magicBody[4];
        public uint bodySize;
    };
}