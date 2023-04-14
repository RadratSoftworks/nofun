using Mono.Cecil.Cil;
using System;

namespace Nofun.PIP2.Encoding
{
    public struct WordEncoding : IEncoding
    {
        public byte opcode;
        public byte d;
        public UInt16 imm;

        public uint Instruction
        {
            set
            {
                opcode = (byte)(value & 0xFF);
                d = (byte)(((value >> 8) & 0xFF) >> IEncoding.RegisterRealIndexShift);
                imm = (UInt16)((value >> 16) & 0xFFFF);
            }
        }
    }
}