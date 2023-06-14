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

namespace Nofun.Module.VMusic
{
    [Module]
    public partial class VMusic
    {
        private const int MUSIC_OK = 0;
        private const int MUSIC_ERR = -1;

        private const int MUSIC_MAXVOLUME = 127;
        private const int MUSIC_MINVOLUME = 0;

        private enum LoadType: uint
        {
            Stream = 0,
            Resource = 1,
            File = 2
        }

        private VMSystem system;
        private SimpleObjectManager<IMusic> musicContainer;

        public VMusic(VMSystem system)
        {
            this.system = system;
            this.musicContainer = new();
        }

        [ModuleCall]
        private int vMusicLoad(int handle, int type)
        {
            try
            {
                switch ((LoadType)type)
                {
                    case LoadType.Resource:
                    {
                        IVMHostStream stream = system.VMStreamModule.Open("", (uint)StreamFlags.Read | (uint)StreamType.Resource | (uint)(handle << 16));
                        if (stream != null)
                        {
                            return musicContainer.Add(system.AudioDriver.LoadMusic(new StandardStreamVM(stream)));
                        }

                        break;
                    }

                    default:
                    {
                        throw new NotImplementedException($"Unsupported music load type {type}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogClass.VMusic, $"Failed to load music (err={ex.ToString()}");    
            }
            
            return MUSIC_ERR;
        }

        [ModuleCall]
        private int vMusicCtrlEx(int handle, NativeMusicControlCode control, int parameters)
        {
            IMusic musicPiece = musicContainer.Get(handle);
            if (musicPiece == null)
            {
                Logger.Error(LogClass.VSound, $"Music handle={handle} is invalid! Control failed!");
                return MUSIC_ERR;
            }

            switch (control)
            {
                case NativeMusicControlCode.Play:
                    musicPiece.Play();
                    break;

                case NativeMusicControlCode.Stop:
                    musicPiece.Stop();
                    break;

                case NativeMusicControlCode.Volume:
                    if (parameters == -1)
                    {
                        return (int)(musicPiece.Volume * MUSIC_MAXVOLUME);
                    }
                    else
                    {
                        musicPiece.Volume = parameters / MUSIC_MAXVOLUME;
                        break;
                    }

                default:
                    Logger.Error(LogClass.VSound, $"Unimplemented music control command: {control}");
                    return MUSIC_ERR;
            }

            return MUSIC_OK;
        }

        [ModuleCall]
        private int vMusicCtrl(int handle, NativeMusicControlCode control)
        {
            return vMusicCtrlEx(handle, control, 0);
        }

        [ModuleCall]
        private int vMusicDisposeHandle(int handle)
        {
            if (handle == 0)
            {
                return MUSIC_OK;
            }

            musicContainer.Remove(handle);
            return MUSIC_OK;
        }

        [ModuleCall]
        private int vMusicGetHandle(VMPtr<byte> musicData)
        {
            try
            {
                // Get as far as possible
                unsafe
                {
                    byte* musicDataPtr = musicData.AsPointer(system.Memory);
                    using (UnmanagedMemoryStream stream = new UnmanagedMemoryStream(musicDataPtr, musicData.RemainingMemoryFromThis(system.Memory)))
                    {
                        return musicContainer.Add(system.AudioDriver.LoadMusic(stream));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LogClass.VMusic, $"Failed to load music (err={ex.ToString()}");
                return -1;
            }
        }

        [ModuleCall]
        int vMusicInit()
        {
            return MUSIC_OK;
        }
    }
}