using System.Runtime.InteropServices;

namespace Nofun.PIP2.Translator
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TranslatorOptions
    {
        [MarshalAs(UnmanagedType.I1)]
        [FieldOffset(0)]
        public bool divideByZeroResultZero;
        [MarshalAs(UnmanagedType.I1)]
        [FieldOffset(1)]
        public bool enableCache;
        [MarshalAs(UnmanagedType.LPStr)]
        [FieldOffset(8)]
        public string cacheRootPath;
        [FieldOffset(16)]
        public uint textBase;
        [FieldOffset(20)]
        public uint entryPoint;
    }
}