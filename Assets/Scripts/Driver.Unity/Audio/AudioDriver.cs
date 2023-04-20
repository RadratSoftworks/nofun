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
                fixed (byte *dataPtr = data)
                {
                    IntPtr handle = TSFMidiRenderer.Load((IntPtr)dataPtr, (uint)data.Length, loop ? 1 : 0);
                    if (handle == null)
                    {
                        throw new Exception("Failed to load the MIDI sound resource!");
                    }

                    TSFMidiSound sound = new TSFMidiSound(handle);
                    activeMidiSounds.Add(sound);

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

                fixed (float* dataPtr = data)
                {
                    TSFMidiRenderer.GetBuffer((IntPtr)dataPtr, data.Length / channels);
                }
            }
        }
    }
}