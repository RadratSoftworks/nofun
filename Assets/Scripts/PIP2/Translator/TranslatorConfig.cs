using System;

namespace Nofun.PIP2.Translator
{
    public struct TranslatorConfig
    {
        public IntPtr memoryBase;
        public ulong memorySize;
        public IntPtr poolItemsBase;
        public ulong poolItemsCount;
        public IntPtr stackAllocateFunction;
        public IntPtr stackFreeFunction;
    }
}