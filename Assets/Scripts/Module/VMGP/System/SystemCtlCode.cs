namespace Nofun.Module.VMGP
{
    public enum SystemCtlCode : uint
    {
        SetVibrate = 1,
        OnScreenInput = 2,
        Orientation = 3,
        GetCulture = 4,
        OpenURL = 5,
        Vendor = 0x80000000
    }
}