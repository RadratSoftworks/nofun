using Nofun.Driver.Audio;
using Nofun.Driver.Audio.Converter;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nofun.Driver.Unity.Audio
{
    public class PCMSound : IPcmSound
    {
        private AudioClip clip;
        private AudioSource source;
        private AudioDriver driver;

        public PCMSound(AudioDriver driver, AudioSource assignedAudioSource, Span<byte> audioData,
            int frequency, int channels, int bitsPerSample, int priority, bool isAdpcm)
        {
            this.driver = driver;

            if (isAdpcm)
            {
                if (bitsPerSample != 16)
                {
                    throw new ArgumentException("Sample bits must be 16 bits for ADPCM audio!");
                }

                clip = AudioClip.Create("ADPCM sound", audioData.Length * 2 / channels, channels,
                    frequency, false);

                short[] dataPCMed = ADPCMToPcm.Convert(audioData);
                using (FileStream test = File.OpenWrite("E:\\testsample2.raw"))
                {
                    var span = MemoryMarshal.Cast<short, byte>(dataPCMed.AsSpan(0));
                    test.Write(span);
                }

                clip.SetData(dataPCMed.Select(sample => (sample < 0) ? (float)-sample / short.MinValue : (float)sample / short.MaxValue).ToArray(), 0);
            }
            else
            {
                if (bitsPerSample == 8)
                {
                    clip = AudioClip.Create("PCM 8bit sound", audioData.Length / channels, channels,
                        frequency, false);

                    clip.SetData(audioData.ToArray().Select(sample => (float)sample / byte.MaxValue).ToArray(), 0);
                }
                else if (bitsPerSample == 16)
                {
                    clip = AudioClip.Create("PCM 8bit sound", audioData.Length / 2 / channels, channels,
                        frequency, false);

                    Span<ushort> sampleShort = MemoryMarshal.Cast<byte, ushort>(audioData);

                    clip.SetData(sampleShort.ToArray().Select(sample => (float)sample / ushort.MaxValue).ToArray(), 0);
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
            driver.ReturnAudioSourceToFreePool(source);
        }

        public void Pause()
        {
            source.Pause();
        }

        public void Play()
        {
            source.Play();
        }

        public void Resume()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }

        public float Volume
        {
            get => source.volume;
            set => source.volume = Math.Clamp(value, 0.0f, 1.0f);
        }

        public float Frequency
        {
            get => source.clip.frequency;
            set => source.pitch = value / source.clip.frequency;
        }
    }
}