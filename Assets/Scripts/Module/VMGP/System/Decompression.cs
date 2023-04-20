using System;
using Nofun.Module.VMStream;
using Nofun.Util;
using Nofun.Util.Logging;
using Nofun.VM;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private int TryLZDecompressContent(BitStream source, Span<byte> dest, byte extendedOffsetBits,
            byte maxOffsetBits)
        {
            uint destPointer = 0;

            while (true)
            {
                ulong flags = source.ReadBits(1);

                if (flags == 1)
                {
                    int v2 = 0;
                    while (v2 < maxOffsetBits && (source.ReadBits(1) == 1))
                    {
                        v2++;
                    }

                    uint copyLength = 2;

                    if (v2 != 0)
                    {
                        copyLength = ((uint)source.ReadBits(v2) | (1u << v2)) + 1;
                    }

                    uint backOffset = 0;

                    if (copyLength == 2)
                    {
                        backOffset = (uint)source.ReadBits(8) + 2;
                    }
                    else
                    {
                        backOffset = (uint)source.ReadBits(extendedOffsetBits) + copyLength;
                    }

                    dest.Slice((int)(destPointer - backOffset), (int)copyLength).CopyTo(
                        dest.Slice((int)destPointer, (int)copyLength));

                    destPointer += copyLength;
                }
                else
                {
                    ulong result = source.ReadBits(8);
                    dest[(int)destPointer++] = (byte)(result & 0xFF);
                }

                if (destPointer >= dest.Length)
                {
                    break;
                }

                if (!source.Valid)
                {
                    break;
                }
            }

            return (int)destPointer;
        }

        private int TryLZDecompressPtr(VMMemory memory, VMPtr<byte> source, VMPtr<byte> dest)
        {
            // Read header
            Span<byte> header = source.AsSpan(memory, 2);
            if ((header[0] != 'L') || (header[1] != 'Z'))
            {
                throw new ArgumentException("The data is not compressed (does not have LZ magic!)");
            }

            source += 2;

            Span<byte> offsetBitsInfo = source.AsSpan(memory, 2);

            source += 2;

            uint uncompressedLength = source.Cast<uint>().Read(memory);

            source += 4;

            uint compressedLength = source.Cast<uint>().Read(memory);

            // Skip the rest, and get to the data decompress
            source += 14;

            return TryLZDecompressContent(new MemoryBitStream(source.AsRawMemory(memory, (int)compressedLength)),
                dest.AsSpan(memory, (int)uncompressedLength), offsetBitsInfo[1], offsetBitsInfo[0]);
        }

        private int TryLZDecompressStream(VMMemory memory, IVMHostStream stream, VMPtr<byte> dest)
        {
            // Read header
            Span<byte> header = stackalloc byte[2];
            if (stream.Read(header, null) != 2)
            {
                return -1;
            }

            if ((header[0] != 'L') || (header[1] != 'Z'))
            {
                throw new ArgumentException("The data is not compressed (does not have LZ magic!)");
            }

            Span<byte> offsetBitsInfo = stackalloc byte[2];
            if (stream.Read(offsetBitsInfo, null) != 2)
            {
                return -1;
            }

            Span<byte> uintBytes = stackalloc byte[4];
            if (stream.Read(uintBytes, null) != 4)
            {
                return -1;
            }

            uint uncompressedLength = BitConverter.ToUInt32(uintBytes);
            if (stream.Read(uintBytes, null) != 4)
            {
                return -1;
            }

            uint compressedLength = BitConverter.ToUInt32(uintBytes);

            // Skip the rest, and get to the data decompress
            stream.Seek(10, StreamSeekMode.Cur);

            return TryLZDecompressContent(new VMStreamBitStream(stream), dest.AsSpan(memory, (int)uncompressedLength), offsetBitsInfo[1], offsetBitsInfo[0]);
        }

        [ModuleCall]
        private int vDecompress(VMPtr<byte> source, VMPtr<byte> dest, int streamHandle, uint readBufSize)
        {
            try
            {
                if (source.IsNull)
                {
                    IVMHostStream stream = system.VMStreamModule.GetStream(streamHandle);
                    if (stream == null)
                    {
                        throw new ArgumentException("The stream handle is not valid!");
                    }

                    return TryLZDecompressStream(system.Memory, stream, dest);
                }
                else
                {
                    return TryLZDecompressPtr(system.Memory, source, dest);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogClass.VMGPSystem, $"Decompression failed with exception: {ex}");
                return -1;
            }
        }
    }
}