using System;
using Nofun.PIP2.Encoding;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void CALLl(UndefinedEncoding encoding)
        {
            UInt32 poolItemNumber = config.ReadDword(Reg[Register.PC]);
            PoolData poolItem = GetPoolData(poolItemNumber);

            if (poolItem.Function != null)
            {
                // Add to skip the pool item number
                Reg[Register.PC] += 4;

                poolItem.Function();
            }
            else if (poolItem.DataType == PoolDataType.ImmInteger)
            {
                // NOTE: Skip the pool item number
                Reg[Register.RA] = Reg[Register.PC] + 4;
                Reg[Register.PC] = (uint)poolItem.ImmediateInteger;
            }
            else
            {
                throw new InvalidOperationException("Trying to call a non-import/non-integer pool data!");
            }
        }

        private void BNEIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] != encoding.s)
            {
                Reg[Register.PC] += (uint)(encoding.t - 1) * 4;
            }
        }

        private void BEQIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] == encoding.s)
            {
                Reg[Register.PC] += (uint)(encoding.t - 1) * 4;
            }
        }

        private void JPr(TwoSourcesEncoding encoding)
        {
            Reg[Register.PC] = Reg[encoding.d];
        }

        private void BLTUI(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] < encoding.s)
            {
                Reg[Register.PC] += (uint)((encoding.t - 1) * 4);
            }
        }
    }
}