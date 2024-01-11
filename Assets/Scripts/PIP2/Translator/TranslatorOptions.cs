using System;
using System.Runtime.InteropServices;

namespace Nofun.PIP2.Translator
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct TranslatorOptions
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool divideByZeroResultZero;
        [MarshalAs(UnmanagedType.I1)]
        public bool enableCache;
        [MarshalAs(UnmanagedType.LPStr)]
        public string cacheRootPath;
        public uint textBase;
        public uint entryPoint;
    }
}