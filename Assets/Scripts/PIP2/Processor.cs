using System;
using System.Collections.Generic;

namespace Nofun.PIP2
{
    public abstract class Processor
    {
        public const int InstructionSize = 4;

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

        public abstract void Run();
        public abstract void Stop();

        public virtual ProcessorContext SaveContext()
        {
            return new ProcessorContext(registers);
        }

        public virtual void LoadContext(ProcessorContext context)
        {
            registers = context.registers;
        }

        /// <summary>
        /// Return a register reference, given a register index.
        /// </summary>
        /// <param name="value">The register indexg.</param>
        /// <returns>The reference to the register</returns>
        /// <exception cref="InvalidOperationException">The register index is out of range</exception>
        public ref UInt32 Reg(UInt32 value)
        {
            if (value > Register.LastReg)
            {
                throw new InvalidOperationException("Trying to access out-of-range register (index=" + value + ")");
            }

            if (value == 0)
            {
                // Always reassign to make sure no one does try to modify it. We sadly can't keep track
                registers[0] = 0;
            }

            return ref registers[value];
        }
    }
}