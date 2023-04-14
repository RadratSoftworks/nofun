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

        private void ProcessRelocation(VMGPPoolItem item, Span<byte> codeData, Span<byte> dataSpan, UInt32 codeAddress, UInt32 dataAddress, UInt32 bssAddress)
        {
            if ((item.itemTarget != 1) && (item.itemTarget != 2))
            {
                throw new InvalidOperationException("Unknown segment relocate type!");
            }

            Span<byte> targetValue = (item.itemTarget == 1) ? codeData.Slice((int)item.targetOffset, 4) : dataSpan.Slice((int)item.targetOffset, 4);
            UInt32 value = BinaryPrimitives.ReadUInt32LittleEndian(targetValue);

            if (item.poolType == PoolItemType.SectionRelativeReloc)
            {
                switch (item.metaOffset)
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

                    case 4:
                        {
                            BinaryPrimitives.WriteUInt32LittleEndian(targetValue, value + bssAddress);
                            break;
                        }

                    default:
                        {
                            throw new InvalidOperationException("Unknown segment to relocate data value to!");
                        }
                }
            }
            else
            {
                // Swap relocation. Replace with base section address + meta
                UInt32 additionValue = item.metaOffset;
                if (item.poolType == PoolItemType.Swap16Reloc)
                {
                    // NOTE: Maybe wrong
                    additionValue &= 0xFFFF;
                }

                uint baseAddr = (item.itemTarget == 1) ? codeAddress : dataAddress;
                BinaryPrimitives.WriteUInt32LittleEndian(targetValue, baseAddr + additionValue);
            }
        }

        private PoolData ProcessGenerateSymbol(VMGPPoolItem poolItem, UInt32 codeAddress, UInt32 dataAddress, UInt32 bssAddress, ICallResolver resolver)
        {
            switch (poolItem.itemTarget)
            {
                case 1:
                    return new PoolData(poolItem.targetOffset + codeAddress);

                case 2:
                    return new PoolData(poolItem.targetOffset + dataAddress);

                case 4:
                    return new PoolData(poolItem.targetOffset + bssAddress);

                default:
                    throw new InvalidProgramException("Unknown pool data produce action: " + poolItem.itemTarget);
            }
        }

        private PoolData ProcessPoolItem(List<VMGPPoolItem> poolItems, List<PoolData> poolDatas, VMGPPoolItem poolItem, Span<byte> codeData, Span<byte> dataSpan, UInt32 codeAddress, UInt32 dataAddress, UInt32 bssAddress, ICallResolver resolver)
        {
            switch (poolItem.poolType)
            {
                case PoolItemType.ImportSymbol:
                    {
                        string value = executable.GetString(poolItem.metaOffset);
                        return new PoolData(resolver.Resolve(value));
                    }

                case PoolItemType.LocalSymbol:
                case PoolItemType.GlobalSymbol:     // Does not really matter to us if it has name or not
                    {
                        return ProcessGenerateSymbol(poolItem, codeAddress, dataAddress, bssAddress, resolver);
                    }

                case PoolItemType.SymbolAdd:
                    {
                        if ((poolItem.metaOffset - 1) < poolItems.Count)
                        {
                            return new PoolData((uint)poolDatas[(int)poolItem.metaOffset - 1].ImmediateInteger + poolItem.targetOffset); 
                        }
                        else
                        {
                            return new PoolData((uint)(ProcessPoolItem(poolItems, poolDatas, poolItems[(int)poolItem.metaOffset - 1], codeData,
                                dataSpan, codeAddress, dataAddress, bssAddress, resolver).ImmediateInteger) + poolItem.targetOffset);
                        }
                    }

                case PoolItemType.SectionRelativeReloc:
                case PoolItemType.Swap32Reloc:
                case PoolItemType.Swap16Reloc:
                    {
                        ProcessRelocation(poolItem, codeData, dataSpan, codeAddress, dataAddress, bssAddress);
                        return null;
                    }


                case PoolItemType.End:
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
                PoolData result = ProcessPoolItem(poolItems, poolDatas, poolItem, codeData, dataSpan, codeAddress, dataAddress, bssAddress, resolver);
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