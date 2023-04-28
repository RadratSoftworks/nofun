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
using UnityEngine;

using Nofun.Driver.Audio;
using System.Collections.Generic;

namespace Nofun.Driver.Unity.Audio
{
    public class AudioDriver : MonoBehaviour, IAudioDriver
    {
        private const string DefaultSoundFontResourceName = "DefaultSfBank";
        private List<TSFMidiSound> activeMidiSounds = new();
        private Queue<AudioSource> freeAudioSources;
        private int systemFrequencyRate;

        [SerializeField]
        private GameObject audioPlayerPrefab;

        [SerializeField]
        private GameObject audioContainer;

        private void LoadBank(byte[] bankData)
        {
            unsafe
            {
                fixed (byte* bankDataPtr = bankData)
                {
                    int startupResult = TSFMidiRenderer.Startup((IntPtr)bankDataPtr, (uint)bankData.Length, AudioSettings.outputSampleRate);
                    if (startupResult < 0)
                    {
                        throw new Exception("Failed to startup TinySoundFont MIDI synthesizer!");
                    }
                }
            }
        }

        private void Start()
        {
            TextAsset asset = Resources.Load<TextAsset>(DefaultSoundFontResourceName);
            if (asset == null)
            {
                throw new MissingComponentException("Default SoundFont bank is missing!");
            }
            LoadBank(asset.bytes);
            freeAudioSources = new();
            systemFrequencyRate = AudioSettings.outputSampleRate;
        }

        private void OnDestroy()
        {
            TSFMidiRenderer.Shutdown();
        }

        public ISound PlaySound(SoundType type, Span<byte> data, bool loop)
        {
            if (type != SoundType.Midi)
            {
                throw new ArgumentException("Sound type other than MIDI has not been implemented!");
            }

            unsafe
            {
                fixed (byte* dataPtr = data)
                {
                    IntPtr handle = TSFMidiRenderer.Load((IntPtr)dataPtr, (uint)data.Length, loop ? 1 : 0);
                    if (handle == null)
                    {
                        throw new Exception("Failed to load the MIDI sound resource!");
                    }

                    TSFMidiSound sound = new TSFMidiSound(handle);

                    lock (activeMidiSounds)
                    {
                        activeMidiSounds.Add(sound);
                    }

                    return sound;
                }
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            unsafe
            {
                // Assume this as audio update function
                // Get all done playing handles and free them
                int count = 0;
                void** freedHandles = (void**)TSFMidiRenderer.GetDonePlayingHandles(new IntPtr(&count));

                lock (activeMidiSounds)
                {
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr compareTarget = (IntPtr)(freedHandles[0]);
                        TSFMidiSound foundRes = activeMidiSounds.Find(sound => sound.NativeHandle == compareTarget);

                        if (foundRes != null)
                        {
                            foundRes.OnDonePlaying();
                            activeMidiSounds.Remove(foundRes);
                        }
                    }
                }

                fixed (float* dataPtr = data)
                {
                    TSFMidiRenderer.GetBuffer((IntPtr)dataPtr, data.Length / channels);
                }
            }
        }

        public uint Capabilities => (uint)SoundCapsFlags.All;

        public SoundConfig SoundConfig
        {
            get
            {
                return new SoundConfig()
                {
                    sampleFrequency = (ushort)systemFrequencyRate,
                    numMixerChannels = 20,
                    numChannels = 2,
                    bitsPerSample = 16
                };
            }
        }

        public bool InitializePCMPlay()
        {
            return true;
        }

        public IPcmSound LoadPCMSound(Span<byte> data, int priority, int frequency, int channelCount,
            int bitsPerSample, bool isAdpcm)
        {
            IPcmSound result = null;
            byte[] dataArr = data.ToArray();

            JobScheduler.Instance.RunOnUnityThreadSync(() =>
            {
                AudioSource freeSource = null;
                if (freeAudioSources.Count == 0)
                {
                    GameObject source = Instantiate(audioPlayerPrefab, audioContainer.transform);
                    freeSource = source.GetComponent<AudioSource>();
                }
                else
                {
                    lock (freeAudioSources)
                    {
                        freeSource = freeAudioSources.Dequeue();
                    }
                }

                result = new PCMSound(this, freeSource, dataArr, frequency, channelCount, bitsPerSample, priority, isAdpcm);
            });

            return result;
        }

        public void ReturnAudioSourceToFreePool(AudioSource source)
        {
            lock (freeAudioSources)
            {
                freeAudioSources.Enqueue(source);
            }
        }
    }
}