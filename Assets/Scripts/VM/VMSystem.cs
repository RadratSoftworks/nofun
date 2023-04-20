using Nofun.Driver.Graphics;
using Nofun.Driver.Input;
using Nofun.Driver.Audio;
using Nofun.Parser;
using Nofun.PIP2;
using System;
using Nofun.Util;

namespace Nofun.VM
{
    public partial class VMSystem
    {
        private const uint ProgramStartOffset = VMMemory.DataAlignment;
        private const int InstructionPerRun = 1000;

        private VMMemory memory;
        private VMCallMap callMap;
        private Processor processor;
        private IGraphicDriver graphicDriver;
        private IInputDriver inputDriver;
        private IAudioDriver audioDriver;
        private VMGPExecutable executable;
        private uint roundedHeapSize;

        private uint stackStartAddress;
        private uint heapAddress;

        private bool shouldStop = false;

        public VMMemory Memory => memory;
        public Processor Processor => processor;
        public IGraphicDriver GraphicDriver => graphicDriver;
        public IInputDriver InputDriver => inputDriver;
        public IAudioDriver AudioDriver => audioDriver;
        public VMGPExecutable Executable => executable;
        public uint HeapStart => heapAddress;
        public uint HeapSize => roundedHeapSize;
        public uint HeapEnd => HeapStart + HeapSize;

        private void LoadModulesAndProgram(VMLoader loader)
        {
            RegisterModules();

            var result = loader.Load(memory.GetMemorySpan((int)ProgramStartOffset, (int)loader.EstimateNeededProgramSize()),
                ProgramStartOffset, callMap);

            processor.Reg[Register.PC] = ProgramStartOffset;
            processor.Reg[Register.SP] = stackStartAddress;

            processor.PoolDatas = result;
        }

        public VMSystem(VMGPExecutable executable, IGraphicDriver graphicDriver, IInputDriver inputDriver, IAudioDriver audioDriver)
        {
            this.graphicDriver = graphicDriver;
            this.inputDriver = inputDriver;
            this.audioDriver = audioDriver;
            this.executable = executable;

            callMap = new VMCallMap(this);

            VMLoader loader = new VMLoader(executable);

            // Make a gap after all program data to avoid weird stack manipulation
            uint totalSize = ProgramStartOffset + loader.EstimateNeededProgramSize() + VMMemory.DataAlignment + executable.Header.stackSize;

            stackStartAddress = totalSize;
            heapAddress = stackStartAddress + VMMemory.DataAlignment;       // Make a gap to avoid weird stack manipulation

            roundedHeapSize = MemoryUtil.AlignUp(executable.Header.dynamicDataHeapSize, VMMemory.DataAlignment);
            totalSize += VMMemory.DataAlignment + roundedHeapSize;

            memory = new VMMemory(totalSize);
            processor = new PIP2.Interpreter.Interpreter(new PIP2.ProcessorConfig()
            {
                ReadCode = memory.ReadMemory32,
                ReadDword = memory.ReadMemory32,
                ReadWord = memory.ReadMemory16,
                ReadByte = memory.ReadMemory8,
                WriteDword = memory.WriteMemory32,
                WriteWord = memory.WriteMemory16,
                WriteByte = memory.WriteMemory8,
                MemoryCopy = memory.MemoryCopy,
                MemorySet = memory.MemorySet
            });

            LoadModulesAndProgram(loader);
        }

        public void Stop()
        {
            shouldStop = true;
            processor.Stop();
        }

        public void Run()
        {
            if (shouldStop)
            {
                return;
            }

            try
            {
                processor.Run(InstructionPerRun);
            }
            catch (Exception ex)
            {
                shouldStop = true;
                throw ex;
            }

            inputDriver.EndFrame();
            graphicDriver.EndFrame();
        }
    }
}