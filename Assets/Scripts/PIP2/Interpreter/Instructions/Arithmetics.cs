using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void ADDQ(TwoSourcesEncoding encoding)
        {
            Reg(encoding.d) = Reg(encoding.s) + BitUtil.SignExtend(encoding.t);
        }

        private void SUB(TwoSourcesEncoding encoding)
        {
            Reg(encoding.d) = Reg(encoding.s) - Reg(encoding.t);
        }
    }
}