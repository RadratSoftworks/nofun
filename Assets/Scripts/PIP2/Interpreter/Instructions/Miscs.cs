using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void EXSH(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = BitUtil.SignExtend(Reg16[encoding.s]);
        }

        private void EXSB(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = BitUtil.SignExtend(Reg8[encoding.s]);
        }

        private void NEG(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = (uint)(-(int)Reg[encoding.s]);
        }
    }
}