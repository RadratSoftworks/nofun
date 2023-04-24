using System;
using System.IO;

namespace Nofun.Parser
{
    public enum PoolItemType
    {
        End = 0,

        // Symbol
        LocalSymbol = 1,
        ImportSymbol = 2,
        GlobalSymbol = 3,

        SectionRelativeReloc = 4,
        Swap16Reloc = 5,
        Swap32Reloc = 6,
        Const32 = 7,
        SymbolAdd = 8
    };

    public class VMGPPoolItem
    {
        public PoolItemType poolType;
        public byte itemTarget;
        public UInt32 metaOffset;
        public UInt32 targetOffset;

        public const int TotalSize = 8;

        public VMGPPoolItem(BinaryReader reader)
        {
            UInt32 segmentWord = reader.ReadUInt32();
            byte segment = (byte)(segmentWord & 0xFF);

            poolType = (PoolItemType)(segment & 0xF);
            itemTarget = (byte)(segment >> 4);

            metaOffset = (segmentWord >> 8);
            targetOffset = reader.ReadUInt32();
        }
    }
}