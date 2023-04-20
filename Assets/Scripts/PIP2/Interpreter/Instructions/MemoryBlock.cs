using Nofun.PIP2.Encoding;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void SYSCPY(TwoSourcesEncoding encoding)
        {
            config.MemoryCopy(Reg[encoding.d], Reg[encoding.s], Reg[encoding.t]);
        }

        private void SYSSET(TwoSourcesEncoding encoding)
        {
            config.MemorySet(Reg[encoding.d], Reg8[encoding.s], Reg[encoding.t]);
        }
    }
}