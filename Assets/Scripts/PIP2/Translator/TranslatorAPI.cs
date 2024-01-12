using System;
using System.Runtime.InteropServices;

namespace Nofun.PIP2.Translator
{
    public class TranslatorAPI
    {
        [DllImport("llvm-pip2", EntryPoint = "vm_engine_create", CharSet = CharSet.Ansi)]
        public static extern IntPtr EngineCreate(string moduleName, IntPtr configPtr, IntPtr optionsPtr);

        [DllImport("llvm-pip2", EntryPoint = "vm_engine_destroy")]
        public static extern void EngineDestroy(IntPtr enginePtr);

        [DllImport("llvm-pip2", EntryPoint = "vm_engine_execute")]
        public static extern void EngineExecute(IntPtr enginePtr, IntPtr handler, IntPtr handlerUserData);

        [DllImport("llvm-pip2", EntryPoint = "vm_engine_reg")]
        public static extern uint EngineGetRegister(IntPtr enginePtr, int reg);

        [DllImport("llvm-pip2", EntryPoint = "vm_engine_set_reg")]
        public static extern void EngineSetRegister(IntPtr enginePtr, int reg, uint value);
    }
}