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
using System.Collections.Generic;

namespace Nofun.PIP2.Assembler
{
    public class Assembler
    {
        public static uint PoolItem(uint index)
        {
            return index;
        }

        public static uint Constant(int value)
        {
            return (1U << 31) | (uint)(value & ~(1 << 30)) | (uint)(((uint)value & (1 << 31)) >> 1);
        }

        private struct InstructionInfo
        {
            public Opcode opcode;
            public byte dest;
            public byte operand1;
            public byte operand2;

            public uint ordinal;
            public Label jumpLabel;
            public bool hasOrdinal;
        }

        private List<InstructionInfo> instructions;

        public Assembler()
        {
            instructions = new();
        }

        public Label L
        {
            get
            {
                return new Label()
                {
                    InstructionOffset = instructions.Count
                };
            }
            set
            {
                value.InstructionOffset = instructions.Count;
            }
        }

        public void LDW(int dest, int baseAddr, uint offsetConstant = 0)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.LDWd,
                dest = (byte)dest,
                operand1 = (byte)baseAddr,
                ordinal = offsetConstant,
                hasOrdinal = true
            });
        }

        public Label BEQ(int lhs, int rhs)
        {
            Label targetJump = new Label();

            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.BEQ,
                dest = (byte)lhs,
                operand1 = (byte)rhs,
                jumpLabel = targetJump,
                hasOrdinal = true
            });

            return targetJump;
        }

        public Label BEQI(int lhs, byte imm)
        {
            Label targetJump = new Label();

            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.BEQI,
                dest = (byte)lhs,
                operand1 = (byte)imm,
                jumpLabel = targetJump,
                hasOrdinal = false
            });

            return targetJump;
        }

        public void CALLr(int register)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.CALLr,
                dest = (byte)register,
                hasOrdinal = false
            });
        }

        public void ADD(int dest, int source1, int source2)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.ADD,
                dest = (byte)dest,
                operand1 = (byte)source1,
                operand2 = (byte)source2,
                hasOrdinal = false
            });
        }

        public void ADDI(int dest, int source1, uint immediate)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.ADDi,
                dest = (byte)dest,
                operand1 = (byte)source1,
                operand2 = 0,
                ordinal = immediate,
                hasOrdinal = true
            });
        }

        public void JP(Label target)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.JPl,
                jumpLabel = target,
                hasOrdinal = true
            });
        }

        public void JPr(int register)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.JPr,
                dest = (byte)register,
                hasOrdinal = false
            });
        }

        public void MOV(int dest, int source)
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.MOV,
                dest = (byte)dest,
                operand1 = (byte)source,
                hasOrdinal = false
            });
        }

        public void PUSH_RA()
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.STORE,
                dest = (byte)0,
                operand1 = (byte)4,
                hasOrdinal = false
            });
        }

        public void RET_RA()
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.RET,
                dest = (byte)0,
                operand1 = (byte)4,
                hasOrdinal = false
            });
        }

        public void KILLTASK()
        {
            instructions.Add(new InstructionInfo()
            {
                opcode = Opcode.KILLTASK,
                dest = 0,
                operand1 = 0,
                hasOrdinal = false
            });
        }

        public uint EstimateWordCount()
        {
            uint total = 0;
            foreach (InstructionInfo info in instructions)
            {
                if (info.hasOrdinal)
                {
                    total += 2;
                }
                else
                {
                    total += 1;
                }
            }
            return total;
        }

        public void Assemble(Span<uint> destMemory)
        {
            int wordPointer = 0;
            List<int> insPos = new();

            foreach (InstructionInfo info in instructions)
            {
                insPos.Add(wordPointer);

                if (info.hasOrdinal)
                {
                    wordPointer += 2;
                }
                else
                {
                    wordPointer += 1;
                }
            }

            int instIndex = 0;

            foreach (InstructionInfo info in instructions)
            {
                uint inst = (uint)info.opcode | (uint)info.dest << 8 | (uint)info.operand1 << 16 | (uint)info.operand2 << 24;
                uint? ordinal = info.hasOrdinal ? info.ordinal : null;

                int currentPos = insPos[instIndex++];

                if (info.jumpLabel != null)
                {
                    if (info.jumpLabel.InstructionOffset < 0)
                    {
                        throw new InvalidOperationException("Jump label has not been set!");
                    }

                    int offset = insPos[info.jumpLabel.InstructionOffset];
                    int diff = offset - currentPos;

                    if (info.hasOrdinal)
                    {
                        ordinal = Constant(diff * 4);
                    }
                    else
                    {
                        inst &= ~0xFF000000;
                        inst |= (uint)((sbyte)diff << 24);
                    }
                }

                destMemory[currentPos] = inst;
                if (ordinal != null)
                {
                    destMemory[currentPos + 1] = ordinal.Value;
                }
            }
        }
    };
}