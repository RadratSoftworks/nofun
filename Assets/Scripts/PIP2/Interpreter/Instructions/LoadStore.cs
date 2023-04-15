using System;
using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void LDI(DestOnlyEncoding encoding)
        {
            UInt32 poolDataIndex = config.ReadDword(Reg(Register.PC));
            PoolData data = GetPoolData(poolDataIndex);

            uint? loadNumber = data?.ImmediateInteger;
            if (loadNumber == null)
            {
                throw new InvalidOperationException("The data to load to LDI from pool is not integer (poolItemIndex=" + poolDataIndex + ")");
            }

            Reg(encoding.d) = (uint)loadNumber;

            if (encoding.d != Register.PC)
            {
                Reg(Register.PC) += 4;
            }
        }

        private void LDQ(WordEncoding encoding)
        {
            Reg(encoding.d) = BitUtil.SignExtend(encoding.imm);
        }

        private void STORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg(Register.SP);

            for (uint i = encoding.d; i <= encoding.s; i++)
            {
                currentSp -= RegSize;
                config.WriteDword(currentSp, Reg(i));
            }

            Reg(Register.SP) = currentSp;
        }

        private void RESTORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg(Register.SP);

            for (uint i = encoding.s; i >= encoding.d; i--)
            {
                Reg(i) = config.ReadDword(currentSp);
                currentSp += RegSize;
            }

            Reg(Register.SP) = currentSp;
        }
    }
}