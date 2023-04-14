using System;

namespace Nofun.PIP2
{
    public class ProcessorContext
    {
        public UInt32[] registers;

        public ProcessorContext(UInt32[] registers)
        {
            this.registers = registers;
        }
    }
}