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

namespace Nofun.PIP2
{
    public abstract class Processor : Processor.IRegIndexer, Processor.IReg16Indexer, Processor.IReg8Indexer, IDisposable
    {
        public interface IRegIndexer
        {
            UInt32 this[uint index] { get; set; }
        };

        public interface IReg16Indexer
        {
            UInt16 this[uint index] { get; set; }
        }

        public interface IReg8Indexer
        {
            byte this[uint index] { get; set; }
        }

        public const int InstructionSize = 4;
        public const int RegSize = 4;

        protected UInt32[] registers;
        protected List<PoolData> poolDatas;
        protected ProcessorConfig config;

        public Processor(ProcessorConfig config)
        {
            registers = new UInt32[Register.TotalReg];
            this.config = config;
        }

        public List<PoolData> PoolDatas
        {
            get { return poolDatas; }
            set { poolDatas = value; }
        }

        protected PoolData GetPoolData(UInt32 numbering)
        {
            if ((numbering == 0) || (numbering > poolDatas.Count))
            {
                throw new IndexOutOfRangeException("Pool data numbering is out-of-range!");
            }

            return poolDatas[(int)numbering - 1];
        }

        public abstract void Run(int instructionPerRun);
        public abstract void Stop();

        public virtual void PostInitialize(uint entryPoint)
        {

        }

        public virtual void Dispose()
        {

        }

        public virtual ProcessorContext SaveContext()
        {
            return new ProcessorContext(registers);
        }

        public virtual void LoadContext(ProcessorContext context)
        {
            registers = context.registers;
        }

        public virtual IRegIndexer Reg => this;
        public virtual IReg16Indexer Reg16 => this;
        public virtual IReg8Indexer Reg8 => this;

        /// <summary>
        /// Return a register reference, given a register index.
        /// </summary>
        /// <param name="value">The register indexg.</param>
        /// <returns>The reference to the register</returns>
        /// <exception cref="InvalidOperationException">The register index is out of range</exception>
        uint IRegIndexer.this[uint index]
        {
            get
            {
                if (index > Register.PC)
                {
                    throw new InvalidOperationException("Trying to access out-of-range register (index=" + index + ")");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException($"Access to 32-bit register is unaligned! (value={index})");
                }

                if (index == 0)
                {
                    return 0;
                }

                return registers[index >> 2];
            }
            set
            {

                if (index > Register.PC)
                {
                    throw new InvalidOperationException("Trying to access out-of-range register (index=" + index + ")");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException($"Access to 32-bit register is unaligned! (value={index})");
                }

                if (index == 0)
                {
                    throw new InvalidOperationException("Register 0 is read-only zero!");
                }

                registers[index >> 2] = value;
            }
        }

        ushort IReg16Indexer.this[uint index]
        {
            get
            {
                if (index > Register.PC)
                {
                    throw new InvalidOperationException("Trying to access out-of-range register (index=" + index + ")");
                }

                if ((index & 1) != 0)
                {
                    throw new InvalidOperationException($"Access to 16-bit register is unaligned! (value={index})");
                }

                ushort ownReg32 = (ushort)(index >> 2);
                ushort byteOff = (ushort)((index & 3) << 3);

                if (ownReg32 == 0)
                {
                    return 0;
                }

                return (ushort)((registers[ownReg32] >> byteOff) & 0xFFFF);
            }
            set
            {
                if (index > Register.PC)
                {
                    throw new InvalidOperationException("Trying to access out-of-range register (index=" + index + ")");
                }

                if ((index & 1) != 0)
                {
                    throw new InvalidOperationException($"Access to 16-bit register is unaligned! (value={index})");
                }

                ushort ownReg32 = (ushort)(index >> 2);
                ushort byteOff = (ushort)((index & 3) << 3);

                if (ownReg32 == 0)
                {
                    throw new InvalidOperationException("Register 0 and 1 is read-only zero!");
                }

                registers[ownReg32] = (registers[ownReg32] & (uint)~(0xFFFF << byteOff)) | (uint)(value << byteOff);
            }
        }

        byte IReg8Indexer.this[uint index]
        {
            get
            {
                if (index > Register.PC)
                {
                    throw new InvalidOperationException("Trying to access out-of-range register (index=" + index + ")");
                }

                ushort ownReg32 = (ushort)(index >> 2);
                byte byteOff = (byte)((index & 3) << 3);

                if (ownReg32 == 0)
                {
                    return 0;
                }

                return (byte)((registers[ownReg32] >> byteOff) & 0xFF);
            }
            set
            {
                if (index > Register.PC)
                {
                    throw new InvalidOperationException("Trying to access out-of-range register (index=" + index + ")");
                }

                ushort ownReg32 = (ushort)(index >> 2);
                byte byteOff = (byte)((index & 3) << 3);

                if (ownReg32 == 0)
                {
                    throw new InvalidOperationException("Register 0 and 1 is read-only zero!");
                }

                registers[ownReg32] = (registers[ownReg32] & (uint)~(0xFF << byteOff)) | (uint)(value << byteOff);
            }
        }

        public abstract int InstructionRan { get; }
    }
}