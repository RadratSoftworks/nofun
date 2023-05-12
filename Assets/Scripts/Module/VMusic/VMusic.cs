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
            long size = stream.Seek(0, StreamSeekMode.End);
            stream.Seek(0, StreamSeekMode.Set);

            byte[] d = new byte[size];
            stream.Read(d, null);

            using (FileStream t = File.OpenWrite("E:\\testraw.mad"))
            {
                t.Write(d);
            }

            SharpMik.Player.MikMod mod = new();
            mod.Init<SharpMik.Drivers.WavDriver>("G:\\test.wav");
            SharpMik.ModuleLoader.RegisterModuleLoader<SharpMik.Loaders.MFXMLoader>();
            mod.Play(new MemoryStream(d));

            int frames = 0;

            while (mod.IsPlaying() && frames++ < 200)
            {
                mod.Update();
            }

            mod.Exit();
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