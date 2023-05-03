/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Nofun.PIP2.Encoding;
using Nofun.Util;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
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

            if (encoding.start == 0)
            {
                currentSp -= RegSize;
                config.WriteDword(currentSp, Reg[Register.RA]);
            }
            else
            {
                for (byte i = encoding.start; i < end; i += 4)
                {
                    currentSp -= RegSize;
                    config.WriteDword(currentSp, Reg[i]);
                }
            }

            Reg[Register.SP] = currentSp;
        }

        private void RESTORE(RangeRegEncoding encoding)
        {
            uint currentSp = Reg[Register.SP];
            int end = encoding.start - encoding.count;

            if (encoding.start == 0)
            {
                // Only save the RA
                Reg[Register.RA] = config.ReadDword(currentSp);
                currentSp += RegSize;
            }
            else
            {
                for (uint i = encoding.start; i > end; i -= 4)
                {
                    Reg[i] = config.ReadDword(currentSp);
                    currentSp += RegSize;
                }
            }

            Reg[Register.SP] = currentSp;
        }
        #endregion
    }
}