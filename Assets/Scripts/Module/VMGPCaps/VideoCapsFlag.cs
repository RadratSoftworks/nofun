namespace Nofun.Module.VMGPCaps
{
    public enum VideoCapsFlag
    {
        Render3D = 0x1000,
        ChangingOrientation = 0x2000,
        All = Render3D | ChangingOrientation
    };
}