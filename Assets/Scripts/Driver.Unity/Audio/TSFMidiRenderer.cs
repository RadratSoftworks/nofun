using System;
using System.Runtime.InteropServices;

namespace Nofun.Driver.Unity.Audio
{
    public static class TSFMidiRenderer
    {
        [DllImport("TSFMidiRenderer", EntryPoint = "nofunTSFStartup")]
        public static extern int Startup(IntPtr soundFontBuffer, uint soundFontSize, int outputFreq);

        [DllImport("TSFMidiRenderer", EntryPoint = "nofunTSFShutdown")]
        public static extern void Shutdown();

        [DllImport("TSFMidiRenderer", EntryPoint = "nofunTSFLoad")]
        public static extern IntPtr Load(IntPtr midiData, uint midiDataSize, int loop = 0);

        [DllImport("TSFMidiRenderer", EntryPoint = "nofunTSFFree")]
        public static extern void Free(IntPtr handle);

        [DllImport("TSFMidiRenderer", EntryPoint = "nofunTSFGetBuffer")]
        public static extern void GetBuffer(IntPtr dataBuffer, int sampleCount);

        [DllImport("TSFMidiRenderer", EntryPoint = "nofunTSFGetDonePlayingHandles")]
        public static extern IntPtr GetDonePlayingHandles(IntPtr count);
    };
}