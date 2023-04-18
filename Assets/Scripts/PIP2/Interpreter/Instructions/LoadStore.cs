using System;
using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private UInt32 FetchLoadStoreImmediate()
        {
            UInt32 infoWord = config.ReadDword(Reg[Register.PC]);
            Reg[Register.PC] += 4;

            // Bit 31 set marking that it's a constant
            if ((infoWord & 0x80000000U) == 0)
            {
                PoolData data = GetPoolData(infoWord);

                uint? loadNumber = data?.ImmediateInteger;
                if (loadNumber == null)
                {
                    throw new InvalidOperationException("The data to load from pool is not integer (poolItemIndex=" + infoWord + ")");
                }

                return (uint)loadNumber;
            }
            else
            {
                return infoWord & 0x7FFFFFFF;
            }
        }

        private void LDI(DestOnlyEncoding encoding)
        {
            Reg[encoding.d] = FetchLoadStoreImmediate();
        }

        private void LDQ(WordEncoding encoding)
        {
            Reg[encoding.d] = BitUtil.SignExtend(encoding.imm);
        }

        private void STORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg[Register.SP];

            for (uint i = encoding.d; i <= encoding.s; i++)
            {
                currentSp -= RegSize;
                config.WriteDword(currentSp, Reg[i]);
            }

            Reg[Register.SP] = currentSp;
        }

        private void RESTORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg[Register.SP];

            for (uint i = encoding.s; i >= encoding.d; i--)
            {
                Reg[i] = config.ReadDword(currentSp);
                currentSp += RegSize;
            }

            Reg[Register.SP] = currentSp;
        }

        private void STHd(TwoSourcesEncoding encoding)
        {
            config.WriteWord(Reg[encoding.s] + FetchLoadStoreImmediate(), Reg16[encoding.d]);
        }

        private void STBd(TwoSourcesEncoding encoding)
        {
            config.WriteByte(Reg[encoding.s] + FetchLoadStoreImmediate(), Reg8[encoding.d]);
        }

        private void LDHu(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = config.ReadWord(Reg[encoding.s] + FetchLoadStoreImmediate());
        }

        private void LDBud(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = config.ReadByte(Reg[encoding.s] + FetchLoadStoreImmediate());
        }
    }
}