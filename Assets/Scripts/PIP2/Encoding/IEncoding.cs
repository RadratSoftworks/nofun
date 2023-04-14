using System;

namespace Nofun.PIP2.Encoding
{
    public interface IEncoding
    {
        public const int RegisterRealIndexShift = 2;

        public UInt32 Instruction
        {
            set;
        }
    }
}