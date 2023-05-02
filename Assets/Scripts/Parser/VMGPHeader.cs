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
using System.IO;

namespace Nofun.Parser
{
    public class VMGPHeader
    {
        public const int TotalSize = 40;

        public byte[] magic;
        public UInt32 dynamicDataHeapSize;
        public UInt32 stackSize;    // Actually 16-bits
        public UInt16 flags;
        public UInt32 codeSize;
        public UInt32 dataSize;
        public UInt32 bssSize;
        public UInt32 resourceSize;
        public UInt32 unk5;
        public UInt32 poolSize;
        public UInt32 stringSize;

        public VMGPHeader(BinaryReader reader)
        {
            Serialize(reader);
        }

        private void Serialize(BinaryReader reader)
        {
            magic = reader.ReadBytes(4);
            if ((magic[0] != 'V') || (magic[1] != 'M') || (magic[2] != 'G') || (magic[3] != 'P'))
            {
                throw new VMGPInvalidHeaderException("The magic is wrong!");
            }

            // Heap and stack size are both in 4-bytes unit
            dynamicDataHeapSize = reader.ReadUInt32() << 2;
            flags = reader.ReadUInt16();
            stackSize = (uint)reader.ReadUInt16() << 2;
            codeSize = reader.ReadUInt32();
            dataSize = reader.ReadUInt32();
            bssSize = reader.ReadUInt32();
            resourceSize = reader.ReadUInt32();
            unk5 = reader.ReadUInt32();
            poolSize = reader.ReadUInt32();
            stringSize = reader.ReadUInt32();
        }
    }
}