namespace Nofun.Driver.Audio
{
    public enum SoundCapsFlags : uint
    {
        Beep = 0x1,
        Wave = 0x2,
        Stereo = 0x4,
        Midi = 0x8,
        Mono = 0x10,
        EightBitPCM = 0x20,
        SixteenBitPCM = 0x40,
        CtrlFrequency = 0x80,
        CtrlPan = 0x100,
        CtrlVolume = 0x200,
        CtrlMasterVolume = 0x400,
        All = Beep | Wave | Stereo | Midi | Mono | EightBitPCM | SixteenBitPCM |
            CtrlFrequency | CtrlPan | CtrlVolume | CtrlMasterVolume
    }
}