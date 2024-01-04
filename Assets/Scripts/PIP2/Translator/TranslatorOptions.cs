using System;
using System.Runtime.InteropServices;

namespace Nofun.PIP2.Translator
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TranslatorOptions
    {
        public bool divideByZeroResultZero;
        public bool enableCache;
        public IntPtr cacheRootPath;
        public uint textBase;
        public uint entryPoint;
    }
}