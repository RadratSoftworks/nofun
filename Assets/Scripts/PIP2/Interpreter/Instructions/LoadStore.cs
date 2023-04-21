using System;
using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private long FetchImmediate()
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
                return (int)((infoWord & 0x7FFFFFFF) | ((infoWord << 1) & 0x80000000));
            }
        }

        private void SkipImmediate()
        {
            Reg[Register.PC] += 4;
        }

        #region Store instructions family
        private void STWd(TwoSourcesEncoding encoding)
        {
            config.WriteDword((uint)(Reg[encoding.s] + FetchImmediate()), Reg[encoding.d]);
        }

        private void STHd(TwoSourcesEncoding encoding)
        {
            config.WriteWord((uint)(Reg[encoding.s] + FetchImmediate()), Reg16[encoding.d]);
        }

        private void STBd(TwoSourcesEncoding encoding)
        {
            config.WriteByte((uint)(Reg[encoding.s] + FetchImmediate()), Reg8[encoding.d]);
        }
        #endregion

        #region Load instructions family
        private void LDI(DestOnlyEncoding encoding)
        {
            Reg[encoding.d] = (uint)FetchImmediate();
        }

        private void LDQ(WordEncoding encoding)
        {
            Reg[encoding.d] = BitUtil.SignExtend(encoding.imm);
        }

        private void LDWd(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = config.ReadDword((uint)(Reg[encoding.s] + FetchImmediate()));
        }

        private void LDHUd(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = config.ReadWord((uint)(Reg[encoding.s] + FetchImmediate()));
        }

        private void LDHd(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = BitUtil.SignExtend(config.ReadWord((uint)(Reg[encoding.s] + FetchImmediate())));
        }

        private void LDBUd(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = config.ReadByte((uint)(Reg[encoding.s] + FetchImmediate()));
        }

        private void LDBd(TwoSourcesEncoding encoding)
        {
            // All operate on 32-bit registers
            Reg[encoding.d] = BitUtil.SignExtend(config.ReadByte((uint)(Reg[encoding.s] + FetchImmediate())));
        }
        #endregion

        #region Stack instructions family
        private void STORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg[Register.SP];
            int end = encoding.start + encoding.count;

            for (byte i = encoding.start; i < end; i += 4)
            {
                currentSp -= RegSize;
                config.WriteDword(currentSp, Reg[i]);
            }

            Reg[Register.SP] = currentSp;
        }

        private void RESTORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg[Register.SP];
            int end = encoding.start - encoding.count;

            for (uint i = encoding.start; i > end; i -= 4)
            {
                Reg[i] = config.ReadDword(currentSp);
                currentSp += RegSize;
            }

            Reg[Register.SP] = currentSp;
        }
        #endregion
    }
}