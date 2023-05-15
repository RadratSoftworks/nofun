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
using Nofun.Module.VMStream;
using Nofun.Util;
using Nofun.Util.Logging;
using Nofun.VM;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private void GetLZCompressInfo(ref VMPtr<byte> source, VMMemory memory, out byte extendedOffsetBits, out byte maxOffsetBits,
            out uint uncompressedSize, out uint compressedSize)
        {
            Span<byte> header = source.AsSpan(memory, 2);
            if ((header[0] != 'L') || (header[1] != 'Z'))
            {
                throw new ArgumentException("The data is not compressed (does not have LZ magic!)");
            }

            source += 2;

            Span<byte> offsetBitsInfo = source.AsSpan(memory, 2);

            source += 2;

            uncompressedSize = source.Cast<uint>().Read(memory);

            source += 4;

            compressedSize = source.Cast<uint>().Read(memory);

            // Skip the rest, and get to the data decompress
            source += 14;

            extendedOffsetBits = offsetBitsInfo[1];
            maxOffsetBits = offsetBitsInfo[0];
        }

        private int TryLZDecompressPtr(VMMemory memory, VMPtr<byte> source, VMPtr<byte> dest)
        {
            // Read header
            GetLZCompressInfo(ref source, memory, out byte extendedOffsetBits, out byte maxOffsetBits,
                out uint uncompressedLength, out uint compressedLength);

            return CompressionUtil.TryLZDecompressContent(new MemoryBitStream(source.AsRawMemory(memory, (int)compressedLength)),
                dest.AsSpan(memory, (int)uncompressedLength), extendedOffsetBits, maxOffsetBits);
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

            return CompressionUtil.TryLZDecompressContent(new VMStreamBitStream(stream), dest.AsSpan(memory, (int)uncompressedLength), offsetBitsInfo[1], offsetBitsInfo[0]);
        }

        [ModuleCall]
        private int vDecompHdr(VMPtr<NativeCompressedFileInfo> infoPtr, VMPtr<byte> source)
        {
            try
            {
                GetLZCompressInfo(ref source, system.Memory, out byte extendedOffsetBits, out byte maxOffsetBits,
                    out uint uncompressedLength, out uint compressedLength);

                if (!infoPtr.IsNull)
                {
                    Logger.Trace(LogClass.VMGPSystem, "Filling compressed file info partly implemented!");

                    Span<NativeCompressedFileInfo> info = infoPtr.AsSpan(system.Memory);
                    info[0].crc16 = 0x1234;
                    info[0].cnt = 0;
                    info[0].option = 0;
                    info[0].offset = 0;
                    info[0].literalSize = 0;
                    info[0].srcSize = compressedLength;
                    info[0].destSize = uncompressedLength;
                }

                return (int)uncompressedLength;
            }
            catch (Exception ex)
            {
                Logger.Error(LogClass.VMGPSystem, $"Get decompressed header failed: {ex}");
                return -1;
            }
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