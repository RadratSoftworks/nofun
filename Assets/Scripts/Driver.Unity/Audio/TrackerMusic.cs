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
using System;
using System.IO;

using SharpMik;
using SharpMik.Loaders;
using SharpMik.Player;

namespace Nofun.Driver.Unity.Audio
{
    public class TrackerMusic : IMusic
    {
        private AudioDriver driver;
        private MikMod player;
        private global::SharpMik.Module playModule;

        private static bool MFXMModuleRegistered = false;

        public static bool IsTrackerSound(Stream data)
        {
            if (data.Length >= 4)
            {
                Span<byte> magic = stackalloc byte[4];
                data.Read(magic);

                bool isTrue = (magic[0] == 'M') && (magic[1] == 'H') && (magic[2] == 'D') && (magic[3] == 'R');
                data.Seek(0, SeekOrigin.Begin);

                return isTrue;
            }

            return false;
        }

        public TrackerMusic(AudioDriver driver, Stream data)
        {
            this.driver = driver;
            this.player = new();

            if (!IsTrackerSound(data))
            {
                throw new InvalidOperationException("Given data is not of MFXM format!");
            }

            // It's actually zero in C, this port just make 0 to false. 0 is usually success code
            if (this.player.Init<SharpMik.FeedToUnityDriver>() == false)
            {
                SharpMik.FeedToUnityDriver unityDrv = this.player.Driver.Driver as SharpMik.FeedToUnityDriver;
                unityDrv.UnityDriver = driver;

                this.player.Driver.MixFrequency = driver.SoundConfig.sampleFrequency;

                if (!MFXMModuleRegistered)
                {        
                    ModuleLoader.RegisterModuleLoader<MFXMLoader>();
                    MFXMModuleRegistered = true;
                }

                playModule = player.LoadModule(data);
            }
            else
            {
                throw new Exception("Failed to initialize SharpMik player!");
            }
        }

        public void Update()
        {
            player.Update();
        }

        public void Dispose()
        {
            if (player.IsPlaying())
            {
                Stop();
            }

            player.Exit();
        }

        public void Pause()
        {
            if (!player.IsPlaying())
            {
                return;
            }

            driver.RemoveActiveTrackerSound(this);
            player.TogglePause();
        }

        public void Play()
        {
            if (player.IsPlaying())
            {
                return;
            }
            
            player.Play(playModule);
            driver.AddActiveTrackerSound(this);
        }

        public void Resume()
        {
            if (player.IsPlaying())
            {
                return;
            }

            Play();
            driver.AddActiveTrackerSound(this);
        }

        public void Stop()
        {
            if (!player.IsPlaying())
            {
                return;
            }

            driver.RemoveActiveTrackerSound(this);
            player.Stop();
        }

        public float Volume
        {
            get => player.Driver.GlobalVolume / 128.0f;
            set => player.Driver.GlobalVolume = (byte)(value * 128.0f);
        }
    }
}