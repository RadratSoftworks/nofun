using Mono.Cecil.Cil;
using System;

namespace Nofun.PIP2.Encoding
{
    public struct UndefinedEncoding : IEncoding
    {
        public UInt32 value;

        public uint Instruction
        {
            set
            {
                this.value = value;
            }
        }
    }
}