using Nofun.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nofun.Parser
{
    public class VMGPExecutable : IDisposable
    {
        private class VMGPResourceInfo
        {
            public uint offset;
            public uint size;
            public byte[] data;

            public VMGPResourceInfo(uint offset)
            {
                this.offset = offset;
                size = 0;
            }
        };

        protected VMGPHeader header;
        protected BinaryReader reader;

        private UInt32 codeSectionOffset;
        private UInt32 dataSectionOffset;
        private UInt32 resourceSectionOffset;
        private UInt32 stringSectionOffset;
        private List<VMGPPoolItem> poolItems;

        private List<VMGPResourceInfo> resourceInfos;

        public VMGPExecutable(Stream fileStream)
        {
            Serialize(fileStream);

            resourceInfos = new();

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

            reader.BaseStream.Seek(resourceSectionOffset, SeekOrigin.Begin);
            while (true)
            {
                uint offset = reader.ReadUInt32();
                if (offset == 0)
                {
                    // Reached the end of the list section, so we should use the section size
                    var previousInfo = resourceInfos[resourceInfos.Count - 1];
                    resourceInfos[resourceInfos.Count - 1].size = poolSectionOffset - previousInfo.offset;

                    break;
                }

                uint offsetRelativeFile = offset + resourceSectionOffset;

                if (resourceInfos.Count > 0)
                {
                    var previousInfo = resourceInfos[resourceInfos.Count - 1];
                    resourceInfos[resourceInfos.Count - 1].size = offsetRelativeFile - previousInfo.offset;
                }

                resourceInfos.Add(new VMGPResourceInfo(offsetRelativeFile));
            }
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

        public byte[] GetResourceData(UInt32 resourceIndex)
        {
            var resourceInfo = resourceInfos[(int)resourceIndex];
            if (resourceInfo.data != null)
            {
                return resourceInfo.data;
            }

            reader.BaseStream.Seek(resourceInfo.offset, SeekOrigin.Begin);

            byte[] resInfo = reader.ReadBytes((int)resourceInfo.size);

            if ((resInfo.Length > 2) && (resInfo[0] == 'L') && (resInfo[1] == 'Z')) {
                // LZ77 compressed resource
                throw new InvalidDataException("Compressed resource data is not yet supported!");
            }

            resourceInfos[(int)resourceIndex].data = resInfo;
            return resInfo;
        }

        void IDisposable.Dispose()
        {
            reader.Dispose();
        }

        public VMGPHeader Header => header;
        public List<VMGPPoolItem> PoolItems => poolItems;
    }
}