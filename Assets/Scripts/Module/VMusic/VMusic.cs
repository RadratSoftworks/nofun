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

using Nofun.Driver.Audio;
using Nofun.Module.VMStream;
using Nofun.Util.Logging;
using Nofun.Util;
using Nofun.VM;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMusic
{
    [Module]
    public partial class VMusic
    {
        private const int MUSIC_OK = 0;
        private const int MUSIC_ERR = -1;

        private enum LoadType: uint
        {
            Stream = 0,
            Resource = 1,
            File = 2
        }

        private VMSystem system;

        public VMusic(VMSystem system)
        {
            this.system = system;
        }

        private void ParseMusicStream(IVMHostStream stream)
        {
            Span<NativeMusicHeader> header = stackalloc NativeMusicHeader[1];
            int headerSize = Marshal.SizeOf<NativeMusicHeader>();

            if (stream.Read(MemoryMarshal.Cast<NativeMusicHeader, byte>(header), null) != headerSize)
            {
                throw new InvalidDataException("Failed to read music header!");
            }

            int dataLeft = header[0].totalDataSize - header[0].songLength - headerSize;
            Span<ushort> wordDataUnk = stackalloc ushort[header[0].songLength];

            if (dataLeft > 0)
            {
                if (dataLeft != header[0].songLength * 2)
                {
                    throw new InvalidDataException("Remaining data size not equal to word data size!");
                }

                if (stream.Read(MemoryMarshal.Cast<ushort, byte>(wordDataUnk), null) != dataLeft)
                {
                    throw new InvalidDataException("Failed to read word data!");
                }
            }
            else
            {
                wordDataUnk.Fill(header[0].defaultRowCount);
            }

            Span<byte> songData = stackalloc byte[header[0].songLength];
            if (stream.Read(songData, null) != header[0].songLength)
            {
                throw new InvalidDataException("Failed to read song data!");
            }

            // Start reading shit
            Span<short> maxAndExtendedOffsetBits = stackalloc short[2];
            uint compressedSize = 0;

            if (stream.Read(MemoryMarshal.Cast<short, byte>(maxAndExtendedOffsetBits), null) != 4)
            {
                throw new InvalidDataException("Failed to read max and extended offset bits!");
            }

            if (stream.Read(MemoryMarshal.Cast<uint, byte>(MemoryMarshal.CreateSpan(ref compressedSize, 1)), null) != 4)
            {
                throw new InvalidDataException("Failed to read compressed data size!");
            }

            Memory<byte> compressedData = new byte[compressedSize];
            if (stream.Read(compressedData.Span, null) != compressedSize)
            {
                throw new InvalidDataException("Failed to read unknown compressed data");
            }

            int test = 10;
        }

        [ModuleCall]
        private int vMusicLoad(int handle, int type)
        {
            switch ((LoadType)type)
            {
                case LoadType.Resource:
                {
                    IVMHostStream stream = system.VMStreamModule.Open("", (uint)StreamFlags.Read | (uint)StreamType.Resource | (uint)(handle << 16));
                    if (stream != null)
                    {
                        ParseMusicStream(stream);
                    }
                    break;
                }
            }
            Logger.Trace(LogClass.VMusic, "Music load stubbed");
            return MUSIC_ERR;
        }

        [ModuleCall]
        private int vMusicCtrlEx(int handle)
        {
            Logger.Trace(LogClass.VMusic, "Music control extended stubbed");
            return MUSIC_ERR;
        }

        [ModuleCall]
        private int vMusicDisposeHandle(int handle)
        {
            Logger.Trace(LogClass.VMusic, "Music dispose handle stubbed");
            return MUSIC_OK;
        }

        [ModuleCall]
        private int vMusicCtrl(int handle)
        {
            Logger.Trace(LogClass.VMusic, "Music control stubbed");
            return MUSIC_ERR;
        }

        [ModuleCall]
        private int vMusicGetHandle(VMPtr<byte> musicData)
        {
            NativeMusicHeader musicSpan = musicData.Cast<NativeMusicHeader>().Read(system.Memory);
            return MUSIC_ERR;
        }

        [ModuleCall]
        int vMusicInit()
        {
            Logger.Trace(LogClass.VMusic, "Music initialization stubbed");
            return MUSIC_OK;
        }
    }
}