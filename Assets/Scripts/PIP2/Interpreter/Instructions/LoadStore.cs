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
    }
}