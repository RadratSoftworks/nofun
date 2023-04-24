namespace Nofun.Module.VMGPCaps
{
    public enum SystemCapsFlags : uint
    {
        Unicode = 1,
        Vibrate = 4,
        BigEndian = 0x10,
        OpenURL = 0x20,

        MordenDeviceCapsExceptEndian = Unicode | Vibrate | OpenURL
    }
}