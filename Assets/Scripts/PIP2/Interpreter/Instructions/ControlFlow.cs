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
                Reg[Register.RA] = Reg[Register.PC] + 4;
                Reg[Register.PC] = (uint)poolItem.ImmediateInteger;
            }
            else
            {
                throw new InvalidOperationException("Trying to call a non-import/non-integer pool data!");
            }
        }
    }
}