namespace Nofun.Module.VMStream
{
    public enum StreamFlags : uint
    {
        Read = 0x100,
        Write = 0x200,
        ReadWrite = Read | Write,
        Binary = 0x400,
        Create = 0x800,
        Trunc = 0x1000,
        MustNotExistBefore = 0x2000,
        Deleted = 0x4000,
        WaitAccept = 0x8000
    }
}