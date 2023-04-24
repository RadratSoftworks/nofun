using System;

namespace Nofun.Driver.Audio
{
    public interface IAudioDriver
    {
        public ISound PlaySound(SoundType type, Span<byte> data, bool loop);

        public uint Capabilities { get; }

        public SoundConfig SoundConfig { get; }

        public bool InitializePCMPlay();
    }
}