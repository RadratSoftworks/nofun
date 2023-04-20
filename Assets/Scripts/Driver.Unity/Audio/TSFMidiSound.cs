using Nofun.Driver.Audio;
using System;

namespace Nofun.Driver.Unity.Audio
{
    public class TSFMidiSound : ISound
    {
        private IntPtr nativeHandle;
        private bool needManualFree = true;

        public IntPtr NativeHandle => nativeHandle;

        public TSFMidiSound(IntPtr nativeHandle)
        {
            this.nativeHandle = nativeHandle;
        }

        public void OnDonePlaying()
        {
            needManualFree = false;
        }

        public void Stop()
        {
            if (needManualFree)
            {
                TSFMidiRenderer.Free(nativeHandle);
            }
        }
    }
}