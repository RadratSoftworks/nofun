using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void ADDQ(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] + BitUtil.SignExtend(encoding.t);
        }

        private void ADDHi(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] + BitUtil.SignExtendToShort(encoding.t));
        }

        private void SUB(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] - Reg[encoding.t];
        }

        private void SRAH(TwoSourcesEncoding encoding)
        {
            // First, cast to short to perform an arthimetic shift to keep sign bit
            // Then, we can cast back to ushort
            Reg16[encoding.d] = (ushort)((short)Reg16[encoding.s] >> encoding.t);
        }

        private void MOVB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = Reg8[encoding.s];
        }

        private void MOV(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s];
        }
    }
}