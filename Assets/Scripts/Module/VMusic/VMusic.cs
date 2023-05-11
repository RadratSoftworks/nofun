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

        [ModuleCall]
        private int vMusicLoad(int handle, int type)
        {
            switch ((LoadType)type)
            {
                case LoadType.Resource:
                {
                    IVMHostStream test = system.VMStreamModule.Open("", (uint)StreamFlags.Read | (uint)StreamType.Resource | (uint)(handle << 16));
                    if (test != null)
                    {
                        Span<NativeMusicHeader> header = stackalloc NativeMusicHeader[1];
                        test.Read(MemoryMarshal.Cast<NativeMusicHeader, byte>(header), null);
                        int asd = 5;

                        Span<byte> bData = stackalloc byte[header[0].unkDataSize];
                            test.Read(bData, null);

                            using (FileStream fs = File.OpenWrite("E:\\testtest.rwrw"))
                            {
                                fs.Write(bData);
                            }
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