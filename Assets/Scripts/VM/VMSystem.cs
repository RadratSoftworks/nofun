using Nofun.Driver.Graphics;
using Nofun.Parser;
using Nofun.PIP2;

namespace Nofun.VM
{
    public partial class VMSystem
    {
        private const uint ProgramStartOffset = VMMemory.DataAlignment;
        private const uint HeapSize = VMMemory.DataAlignment * 10;

        private VMMemory memory;
        private VMCallMap callMap;
        private Processor processor;
        private IGraphicDriver graphicDriver;

        public VMMemory Memory => memory;
        public Processor Processor => processor;
        public IGraphicDriver GraphicDriver => graphicDriver;

        private void LoadModulesAndProgram(VMLoader loader)
        {
            RegisterModules();

            var result = loader.Load(memory.GetMemorySpan((int)VMMemory.DataAlignment, (int)loader.EstimateNeededProgramSize()),
                ProgramStartOffset, callMap);


            processor.Reg(Register.PC) = ProgramStartOffset;
            processor.PoolDatas = result;
        }

        public VMSystem(VMGPExecutable executable, IGraphicDriver graphicDriver)
        {
            this.graphicDriver = graphicDriver;
            callMap = new VMCallMap(this);

            VMLoader loader = new VMLoader(executable);
            uint totalSize = ProgramStartOffset + loader.EstimateNeededProgramSize() + HeapSize;

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
            processor.Run();
            graphicDriver.EndFrame();
        }
    }
}