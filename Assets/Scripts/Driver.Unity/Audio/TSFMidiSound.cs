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

namespace Nofun.Driver.Unity.Audio
{
    public class TSFMidiSound : ISound
    {
        private IntPtr nativeHandle;
        private bool needManualFree = true;
        private AudioDriver audioDriver;

        public IntPtr NativeHandle => nativeHandle;

        public TSFMidiSound(AudioDriver driver, IntPtr nativeHandle)
        {
            this.nativeHandle = nativeHandle;
            this.audioDriver = driver;
        }

        public void OnDonePlaying()
        {
            needManualFree = false;
        }

        public void Stop()
        {
            if (needManualFree)
            {
                JobScheduler.Instance.RunOnUnityThread(() =>
                {
                    audioDriver.RemoveMidiSound(this);
                    TSFMidiRenderer.Free(nativeHandle);
                });
            }
        }
    }
}