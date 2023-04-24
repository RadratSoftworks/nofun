using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        #region Add instruction family
        private void ADD(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] + Reg[encoding.t];
        }

        private void ADDi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] + (uint)FetchImmediate();
        }

        private void ADDQ(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] + BitUtil.SignExtend(encoding.t);
        }

        private void ADDH(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] + Reg16[encoding.t]);
        }

        private void ADDHi(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] + BitUtil.SignExtendToShort(encoding.t));
        }

        private void ADDB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] + Reg8[encoding.t]);
        }

        private void ADDBi(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] + encoding.t);
        }
        #endregion

        #region Subtract instructions family
        private void SUB(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] - Reg[encoding.t];
        }

        private void SUBi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] - (uint)FetchImmediate();
        }

        private void SUBH(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] - Reg16[encoding.t]);
        }

        private void SUBB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] - Reg8[encoding.t]);
        }
        #endregion

        #region Multiply instructions family
        private void MUL(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] * Reg[encoding.t];
        }

        private void MULi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] * (uint)FetchImmediate();
        }

        private void MULQ(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] * encoding.t;
        }
        #endregion

        #region Divide instructions family
        private void DIV(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = (uint)((int)Reg[encoding.s] / (int)Reg[encoding.t]);
        }

        private void DIVU(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] / Reg[encoding.t];
        }

        private void DIVi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = (uint)((int)Reg[encoding.s] / (int)FetchImmediate());
        }

        private void DIVUi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] / (uint)FetchImmediate();
        }
        #endregion

        #region Shift instructions family
        private void SLL(TwoSourcesEncoding encoding)
        {
            // NOTE: Must case to byte for C#. May revisit if bugged
            Reg[encoding.d] = Reg[encoding.s] << (byte)Reg[encoding.t];
        }

        private void SLLi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] << encoding.t;
        }

        private void SLLH(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] << encoding.t);
        }

        private void SLLB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] << encoding.t);
        }

        private void SRL(TwoSourcesEncoding encoding)
        {
            // NOTE: Must case to byte for C#. May revisit if bugged
            Reg[encoding.d] = Reg[encoding.s] >> (byte)Reg[encoding.t];
        }

        private void SRLi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] >> encoding.t;
        }

        private void SRLH(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] >> encoding.t);
        }

        private void SRLB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] >> encoding.t);
        }

        private void SRAi(TwoSourcesEncoding encoding)
        {
            // First, cast to short to perform an arthimetic shift to keep sign bit
            // Then, we can cast back to ushort
            Reg[encoding.d] = (uint)((int)Reg[encoding.s] >> encoding.t);
        }

        private void SRAH(TwoSourcesEncoding encoding)
        {
            // First, cast to short to perform an arthimetic shift to keep sign bit
            // Then, we can cast back to ushort
            Reg16[encoding.d] = (ushort)((short)Reg16[encoding.s] >> encoding.t);
        }

        private void SRAB(TwoSourcesEncoding encoding)
        {
            // First, cast to sbyte to perform an arthimetic shift to keep sign bit
            // Then, we can cast back to sbyte
            Reg8[encoding.d] = (byte)((sbyte)Reg8[encoding.s] >> encoding.t);
        }
        #endregion

        #region And instructions family
        private void AND(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] & Reg[encoding.t];
        }

        private void ANDi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] & (uint)FetchImmediate();
        }

        private void ANDHi(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = (ushort)(Reg16[encoding.s] & encoding.t);
        }

        private void ANDB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] & Reg8[encoding.t]);
        }

        private void ANDBi(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] & encoding.t);
        }
        #endregion

        #region Or instructions family
        private void OR(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] | Reg[encoding.t];
        }

        private void ORi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] | (uint)FetchImmediate();
        }

        private void ORB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] | Reg8[encoding.t]);
        }

        private void ORBi(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = (byte)(Reg8[encoding.s] | encoding.t);
        }
        #endregion


        #region XOR instructions family

        private void XORi(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s] ^ (uint)FetchImmediate();
        }
        #endregion

        #region Move instructions family
        private void MOV(TwoSourcesEncoding encoding)
        {
            Reg[encoding.d] = Reg[encoding.s];
        }

        private void MOVH(TwoSourcesEncoding encoding)
        {
            Reg16[encoding.d] = Reg16[encoding.s];
        }

        private void MOVB(TwoSourcesEncoding encoding)
        {
            Reg8[encoding.d] = Reg8[encoding.s];
        }
        #endregion
    }
}