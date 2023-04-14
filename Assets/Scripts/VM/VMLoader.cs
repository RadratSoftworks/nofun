using System;
using System.Buffers.Binary;
using System.Collections.Generic;

using Nofun.Parser;
using Nofun.Util;
using Nofun.PIP2;

namespace Nofun.VM
{
    public class VMLoader
    {
        private VMGPExecutable executable;

        public VMLoader(VMGPExecutable executable)
        {
            this.executable = executable;
        }

        private UInt32 EstimatedCodeSectionSize()
        {
            return MemoryUtil.AlignUp(executable.Header.codeSize, VMMemory.DataAlignment);
        }

        private UInt32 EstimatedDataSize()
        {
            return MemoryUtil.AlignUp(executable.Header.dataSize, VMMemory.DataAlignment);
        }

        private UInt32 EstimatedBssSize()
        {
            return MemoryUtil.AlignUp(executable.Header.bssSize, VMMemory.DataAlignment);
        }

        private UInt32 EstimatedDataSectionSize()
        {
            return EstimatedDataSize() + EstimatedBssSize();
        }

        public UInt32 EstimateNeededProgramSize()
        {
            return EstimatedCodeSectionSize() + EstimatedDataSectionSize();
        }

        private void ProcessInMemoryRelocatePoolItem(VMGPPoolItem item, Span<byte> codeData, Span<byte> dataSpan, UInt32 codeAddress, UInt32 dataAddress)
        {
            if (item.segmentRelocateType != 2)
            {
                throw new InvalidOperationException("Unknown segment relocate type!");
            }

            Span<byte> targetValue = dataSpan.Slice((int)item.extra, 4);
            UInt32 value = BinaryPrimitives.ReadUInt32LittleEndian(targetValue);

            switch (item.segmentOffset)
            {
                case 1:
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(targetValue, value + codeAddress);
                        break;
                    }

                case 2:
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(targetValue, value + dataAddress);
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException("Unknown segment to relocate data value to!");
                    }
            }
        }

        private PoolData ProcessRelocateItemToData(VMGPPoolItem poolItem, UInt32 codeAddress, UInt32 dataAddress, UInt32 bssAddress, ICallResolver resolver)
        {
            switch (poolItem.segmentRelocateType)
            {
                // Import
                case 0:
                    {
                        string value = executable.GetString(poolItem.segmentOffset);
                        return new PoolData(resolver.Resolve(value));
                    }

                case 1:
                    return new PoolData(poolItem.extra + codeAddress);

                case 2:
                    return new PoolData(poolItem.extra + dataAddress);

                case 4:
                    return new PoolData(poolItem.extra + bssAddress);

                default:
                    throw new InvalidProgramException("Unknown pool data produce action: " + poolItem.segmentRelocateType);
            }
        }

        private PoolData ProcessPoolItem(List<VMGPPoolItem> poolItems, VMGPPoolItem poolItem, Span<byte> codeData, Span<byte> dataSpan, UInt32 codeAddress, UInt32 dataAddress, UInt32 bssAddress, ICallResolver resolver)
        {
            switch (poolItem.segmentRelocateCommand)
            {
                case SegmentRelocateCommand.RelocateFromAnotherPoolItem:
                    {
                        PoolData decoded = ProcessPoolItem(poolItems, poolItems[(int)poolItem.segmentOffset - 1], codeData, dataSpan, codeAddress, dataAddress, bssAddress, resolver);
                        if ((decoded == null) || (decoded.DataType != PoolDataType.ImmInteger))
                        {
                            throw new InvalidOperationException("Result of relocation from another pool item is not of integer type!");
                        }
                        return new PoolData((int)poolItem.extra + (int)decoded.ImmediateInteger);
                    }

                case SegmentRelocateCommand.RelocateInMemorySection:
                    {
                        ProcessInMemoryRelocatePoolItem(poolItem, codeData, dataSpan, codeAddress, dataAddress);
                        return null;
                    }

                case SegmentRelocateCommand.RelocatePoolDataToCode:
                case SegmentRelocateCommand.RelocatePoolDataImport:
                    {
                        return ProcessRelocateItemToData(poolItem, codeAddress, dataAddress, bssAddress, resolver);
                    }

                case SegmentRelocateCommand.RelocateNull:
                    {
                        return null;
                    }

                default:
                    {
                        throw new InvalidProgramException("Unrecognised segment relocate command!");
                    }
            }
        }

        private List<PoolData> ProcessPoolItems(List<VMGPPoolItem> poolItems, Span<byte> codeData, Span<byte> dataSpan, UInt32 codeAddress, UInt32 dataAddress, UInt32 bssAddress, ICallResolver resolver)
        {
            List<PoolData> poolDatas = new List<PoolData>();
            foreach (VMGPPoolItem poolItem in poolItems)
            {
                PoolData result = ProcessPoolItem(poolItems, poolItem, codeData, dataSpan, codeAddress, dataAddress, bssAddress, resolver);
                if (result == null)
                {
                    result = new PoolData();
                }
                poolDatas.Add(result);
            }

            return poolDatas;
        }

        public List<PoolData> Load(Span<byte> programMemory, UInt32 baseAddress, ICallResolver callResolver)
        {
            UInt32 codeBaseAddress = baseAddress;
            UInt32 dataBaseAddress = baseAddress + EstimatedCodeSectionSize();
            UInt32 bssBaseAddress = dataBaseAddress + EstimatedDataSize(); 

            Span<byte> codeSpan = programMemory.Slice(0, (int)EstimatedCodeSectionSize());
            Span<byte> dataSpan = programMemory.Slice((int)EstimatedCodeSectionSize(), (int)EstimatedDataSize());
            Span<byte> bssSpan = programMemory.Slice((int)(EstimatedCodeSectionSize() + EstimatedDataSize()), (int)EstimatedBssSize());

            executable.GetCodeSection(codeSpan);
            executable.GetDataSection(dataSpan);

            // Clear the BSS
            bssSpan.Fill(0x00);

            return ProcessPoolItems(executable.PoolItems, codeSpan, dataSpan, codeBaseAddress, dataBaseAddress, bssBaseAddress, callResolver);
        }
    }
}