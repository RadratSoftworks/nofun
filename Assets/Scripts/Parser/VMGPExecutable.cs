using Nofun.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nofun.Parser
{
    public class VMGPExecutable : IDisposable
    {
        protected VMGPHeader header;
        protected BinaryReader reader;

        private UInt32 codeSectionOffset;
        private UInt32 dataSectionOffset;
        private UInt32 resourceSectionOffset;
        private UInt32 stringSectionOffset;
        private List<VMGPPoolItem> poolItems;

        public VMGPExecutable(Stream fileStream)
        {
            Serialize(fileStream);

            codeSectionOffset = VMGPHeader.TotalSize;
            dataSectionOffset = codeSectionOffset + header.codeSize;
            resourceSectionOffset = dataSectionOffset + header.dataSize;
            
            UInt32 poolSectionOffset = resourceSectionOffset + header.resourceSize;
            reader.BaseStream.Seek(poolSectionOffset, SeekOrigin.Begin);

            poolItems = new List<VMGPPoolItem>();

            for (UInt32 i = 0; i < header.poolSize; i++)
            {
                poolItems.Add(new VMGPPoolItem(reader));
            }

            stringSectionOffset = poolSectionOffset + header.poolSize * VMGPPoolItem.TotalSize;
        }

        private void Serialize(Stream fileStream)
        {
            reader = new BinaryReader(fileStream);
            header = new VMGPHeader(reader);
        }

        public void GetCodeSection(Span<byte> codeSection)
        {
            reader.BaseStream.Seek(codeSectionOffset, SeekOrigin.Begin);
            reader.Read(codeSection);
        }

        public void GetDataSection(Span<byte> dataSection)
        {
            reader.BaseStream.Seek(dataSectionOffset, SeekOrigin.Begin);
            reader.Read(dataSection);
        }

        public string GetString(UInt32 offset)
        {
            reader.BaseStream.Seek(stringSectionOffset + offset, SeekOrigin.Begin);
            return StreamUtil.ReadNullTerminatedString(reader);
        }

        void IDisposable.Dispose()
        {
            reader.Dispose();
        }

        public VMGPHeader Header => header;
        public List<VMGPPoolItem> PoolItems => poolItems;
    }
}