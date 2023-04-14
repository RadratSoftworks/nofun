using System;
using System.IO;

namespace Nofun.Parser
{
    public enum SegmentRelocateCommand
    {
        RelocateNull = 0,
        RelocatePoolDataToCode = 1,
        RelocatePoolDataImport = 2,
        RelocateInMemorySection = 4,
        RelocateFromAnotherPoolItem = 8
    };

    public class VMGPPoolItem
    {
        public SegmentRelocateCommand segmentRelocateCommand;
        public byte segmentRelocateType;
        public UInt32 segmentOffset;
        public UInt32 extra;

        public const int TotalSize = 8;

        public VMGPPoolItem(BinaryReader reader)
        {
            UInt32 segmentWord = reader.ReadUInt32();
            byte segment = (byte)(segmentWord & 0xFF);

            segmentRelocateCommand = (SegmentRelocateCommand)(segment & 0xF);
            segmentRelocateType = (byte)(segment >> 4);

            segmentOffset = (segmentWord >> 8);
            extra = reader.ReadUInt32();
        }
    }
}