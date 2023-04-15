using System;
using System.IO;

namespace Nofun.Parser
{
    public class VMGPHeader
    {
        public const int TotalSize = 40;

        public byte[] magic;
        public UInt32 dynamicDataHeapSize;
        public UInt32 stackSize;
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
            stackSize = reader.ReadUInt32() << 2;
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