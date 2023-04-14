using Nofun.PIP2.Encoding;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void SUB(TwoSourcesEncoding encoding)
        {
            Reg(encoding.d) = Reg(encoding.s) - Reg(encoding.t);
        }
    }
}