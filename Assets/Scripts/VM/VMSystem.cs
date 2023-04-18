using Nofun.Driver.Graphics;
using Nofun.Parser;
using Nofun.PIP2;
using System;

namespace Nofun.VM
{
    public partial class VMSystem
    {
        private const uint ProgramStartOffset = VMMemory.DataAlignment;

        private VMMemory memory;
        private VMCallMap callMap;
        private Processor processor;
        private IGraphicDriver graphicDriver;
        private VMGPExecutable executable;

        private uint stackStartAddress;
        private uint heapAddress;

        private bool shouldStop = false;

        public VMMemory Memory => memory;
        public Processor Processor => processor;
        public IGraphicDriver GraphicDriver => graphicDriver;
        public VMGPExecutable Executable => executable;

        private void LoadModulesAndProgram(VMLoader loader)
        {
            RegisterModules();

            var result = loader.Load(memory.GetMemorySpan((int)ProgramStartOffset, (int)loader.EstimateNeededProgramSize()),
                ProgramStartOffset, callMap);

            processor.Reg[Register.PC] = ProgramStartOffset;
            processor.Reg[Register.SP] = stackStartAddress;

            processor.PoolDatas = result;
        }

        public VMSystem(VMGPExecutable executable, IGraphicDriver graphicDriver)
        {
            this.graphicDriver = graphicDriver;
            this.executable = executable;

            callMap = new VMCallMap(this);

            VMLoader loader = new VMLoader(executable);

            // Make a gap after all program data to avoid weird stack manipulation
            uint totalSize = ProgramStartOffset + loader.EstimateNeededProgramSize() + VMMemory.DataAlignment + executable.Header.stackSize;

            stackStartAddress = totalSize;
            heapAddress = stackStartAddress + VMMemory.DataAlignment;       // Make a gap to avoid weird stack manipulation

            totalSize += VMMemory.DataAlignment + executable.Header.dynamicDataHeapSize;

            memory = new VMMemory(totalSize);
            processor = new PIP2.Interpreter.Interpreter(new PIP2.ProcessorConfig()
            {
                ReadCode = memory.ReadMemory32,
                ReadDword = memory.ReadMemory32,
                ReadWord = memory.ReadMemory16,
                ReadByte = memory.ReadMemory8,
                WriteDword = memory.WriteMemory32,
                WriteWord = memory.WriteMemory16,
                WriteByte = memory.WriteMemory8
            });

            LoadModulesAndProgram(loader);
        }

        public void Run()
        {
            if (shouldStop)
            {
                return;
            }

            try
            {
                processor.Run();
            }
            catch (Exception ex)
            {
                shouldStop = true;
                throw ex;
            }

            graphicDriver.EndFrame();
        }
    }
}