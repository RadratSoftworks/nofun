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
using Nofun.Driver.Audio.Converter;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using NoAlloq;

namespace Nofun.Driver.Unity.Audio
{
    public class PCMSound : IPcmSound
    {
        private AudioClip clip;
        private AudioSource source;
        private AudioDriver driver;

        public PCMSound(AudioDriver driver, AudioSource assignedAudioSource, byte[] audioData,
            int frequency, int channels, int bitsPerSample, int priority, bool isAdpcm)
        {
            this.driver = driver;

            if (isAdpcm)
            {
                clip = AudioClip.Create("ADPCM sound", audioData.Length * 2 / channels, channels,
                    frequency, false);

                short[] dataPCMed = ADPCMToPcm.Convert(audioData);
                clip.SetData(dataPCMed.Select(sample => (sample < 0) ? (float)-sample / short.MinValue : (float)sample / short.MaxValue).ToArray(), 0);
            }
            else
            {
                if (bitsPerSample == 8)
                {
                    clip = AudioClip.Create("PCM 8bit sound", audioData.Length / channels, channels,
                        frequency, false);

                    clip.SetData(audioData.Select(sample => (float)sample / byte.MaxValue).ToArray(), 0);
                }
                else if (bitsPerSample == 16)
                {
                    clip = AudioClip.Create("PCM 8bit sound", audioData.Length / 2 / channels, channels,
                        frequency, false);

                    Span<ushort> sampleShort = MemoryMarshal.Cast<byte, ushort>(audioData);

                    clip.SetData(sampleShort.Select(sample => (float)sample / ushort.MaxValue).ToArray(), 0);
                }
                else
                {
                    throw new ArgumentException("Sample bits must be 8 or 16 for PCM audio!");
                }
            }

            this.source = assignedAudioSource;
            this.source.clip = clip;
            this.source.priority = priority;
            this.source.pitch = 1.0f;

            this.driver = driver;
        }

        public void Dispose()
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                driver.ReturnAudioSourceToFreePool(source);
            });
        }

        public void Pause()
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                source.Pause();
            });
        }

        public void Play()
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                source.Play();
            });
        }

        public void Resume()
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                source.Play();
            });
        }

        public void Stop()
        {
            JobScheduler.Instance.RunOnUnityThread(() =>
            {
                source.Stop();
            });
        }

        public float Volume
        {
            get => source.volume;
            set
            {
                JobScheduler.Instance.RunOnUnityThread(() =>
                {
                    source.volume = Math.Clamp(value, 0.0f, 1.0f);
                });
            }
        }

        public float Frequency
        {
            get => source.clip.frequency;
            set
            {
                JobScheduler.Instance.RunOnUnityThread(() =>
                {
                    source.pitch = value / source.clip.frequency;
                });
            }
        }
    }
}