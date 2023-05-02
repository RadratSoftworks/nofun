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

using System;
using Nofun.PIP2.Encoding;
using Nofun.Util;
using Nofun.Util.Logging;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private void CALLr(DestOnlyEncoding encoding)
        {
            Reg[Register.RA] = Reg[Register.PC];
            Reg[Register.PC] = Reg[encoding.d];
        }

        private void CALLl(UndefinedEncoding encoding)
        {
            UInt32 infoWord = config.ReadDword(Reg[Register.PC]);
            if (IsDwordRawImmediate(ref infoWord))
            {
                Reg[Register.RA] = Reg[Register.PC] + 4;
                Reg[Register.PC] = (uint)(Reg[Register.PC] + (int)infoWord - 4);
            }
            else
            {
                PoolData poolItem = GetPoolData(infoWord);

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
        }

        #region Branch normal variant instructions family
        private void BEQ(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] == Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BNE(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] != Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BGT(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] > (int)Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BGTU(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] > Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BGE(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] >= (int)Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BGEU(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] >= Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BLT(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] < (int)Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BLTU(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] < Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BLE(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] <= (int)Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }

        private void BLEU(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] <= Reg[encoding.s])
            {
                // Subtract of PC increment before, immediate not need
                Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
            }
            else
            {
                SkipImmediate();
            }
        }
        #endregion

        #region Branch 32 immediate variant instructions family
        private void BEQI(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] == BitUtil.SignExtend(encoding.s))
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BNEI(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] != BitUtil.SignExtend(encoding.s))
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BGEI(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] >= (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BGTI(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] > (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BGTUI(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] > encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLEI(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] <= (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLEUI(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] <= encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLTUI(TwoSourcesEncoding encoding)
        {
            if (Reg[encoding.d] < encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLTI(TwoSourcesEncoding encoding)
        {
            if ((int)Reg[encoding.d] < (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }
        #endregion

        #region Branch 8 immediate variant instructions family
        private void BEQIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] == encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BNEIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] != encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BGEIB(TwoSourcesEncoding encoding)
        {
            if ((sbyte)Reg8[encoding.d] >= (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BGTIB(TwoSourcesEncoding encoding)
        {
            if ((sbyte)Reg8[encoding.d] > (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BGTUIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] > encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLEIB(TwoSourcesEncoding encoding)
        {
            if ((sbyte)Reg8[encoding.d] <= (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLEUIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] <= encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLTIB(TwoSourcesEncoding encoding)
        {
            if ((sbyte)Reg8[encoding.d] < (sbyte)encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }

        private void BLTUIB(TwoSourcesEncoding encoding)
        {
            if (Reg8[encoding.d] < encoding.s)
            {
                Reg[Register.PC] = (uint)(Reg[Register.PC] + ((sbyte)encoding.t - 1) * 4);
            }
        }
        #endregion

        #region Jump instructions family
        private void JPr(TwoSourcesEncoding encoding)
        {
            Reg[Register.PC] = Reg[encoding.d];
        }

        private void JPl(TwoSourcesEncoding encoding)
        {
            // Register is retrieved before FetchImmediate, so we don't need to subtract by 8, just 4
            Reg[Register.PC] = (uint)(Reg[Register.PC] + FetchImmediate() - 4);
        }
        #endregion

        private void RET(RangeRegEncoding encoding)
        {
            RESTORE(encoding);
            Reg[Register.PC] = Reg[Register.RA];
        }
    }
}