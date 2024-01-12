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

using Nofun.Driver.Graphics;
using Nofun.Driver.Input;
using Nofun.Driver.Audio;
using Nofun.Driver.Time;
using Nofun.Driver.UI;
using Nofun.Parser;
using Nofun.PIP2;
using Nofun.PIP2.Assembler;
using System;
using Nofun.Util;
using System.IO;
using System.Runtime.InteropServices;
using Nofun.PIP2.Translator;
using Nofun.Settings;
using UnityEngine;

namespace Nofun.VM
{
    public partial class VMSystem : IDisposable
    {
        public const uint ProgramStartOffset = VMMemory.DataAlignment;
        private const int InstructionPerRun = 10000;

        private VMMemory memory;
        private VMCallMap callMap;
        private VMMetaInfoReader metaInfoReader;
        private Processor processor;
        private IGraphicDriver graphicDriver;
        private IInputDriver inputDriver;
        private IAudioDriver audioDriver;
        private ITimeDriver timeDriver;
        private IUIDriver uiDriver;
        private VMGPExecutable executable;
        private uint roundedHeapSize;
        private string persistentDataPath;
        private string llvmCachePath;

        private uint constructorListAddress;
        private uint destructorListAddress;
        private uint listRunAddress;
        private uint stackStartAddress;
        private uint heapAddress;

        private bool shouldStop = false;
        private string gameName;

        public GameSetting GameSetting { get; set; }
        public SystemVersion Version => GameSetting.systemVersion;

        public VMMemory Memory => memory;
        public Processor Processor => processor;
        public IGraphicDriver GraphicDriver => graphicDriver;
        public IInputDriver InputDriver => inputDriver;
        public IAudioDriver AudioDriver => audioDriver;
        public ITimeDriver TimeDriver => timeDriver;
        public IUIDriver UIDriver => uiDriver;
        public VMGPExecutable Executable => executable;
        public uint HeapStart => heapAddress;
        public uint HeapSize => roundedHeapSize;
        public uint HeapEnd => HeapStart + HeapSize;
        public string PersistentDataPath => persistentDataPath;
        public string GameName => gameName;

        private void CreateCallListInvokeCode(Span<uint> memorySpan)
        {
            Assembler assembler = new Assembler();

            assembler.PUSH_RA();
            assembler.MOV(Register.S1, Register.P0);
            assembler.MOV(Register.S2, Register.P1);

            Label loopPoint = assembler.L;
            assembler.LDW(Register.S0, Register.S1, Assembler.Constant(0));
            Label targetJump = assembler.BEQI(Register.S0, 0);

            assembler.CALLr(Register.S0);
            assembler.ADDI(Register.S1, Register.S1, Assembler.Constant(4));
            assembler.JP(loopPoint);
            assembler.L = targetJump;

            assembler.CALLr(Register.S2);
            assembler.RET_RA();

            assembler.Assemble(memorySpan);
        }

        private void LoadModulesAndProgram(VMLoader loader)
        {
            RegisterModules();

            var result = loader.Load(memory.GetMemorySpan((int)ProgramStartOffset, (int)loader.EstimateNeededProgramSize()),
                ProgramStartOffset, callMap);

            constructorListAddress = result.Find(poolData => poolData?.Name?.Equals("~C", StringComparison.OrdinalIgnoreCase) ?? false)?.ImmediateInteger ?? 0;
            destructorListAddress = result.Find(poolData => poolData?.Name?.Equals("~D", StringComparison.OrdinalIgnoreCase) ?? false)?.ImmediateInteger ?? 0;

            if ((constructorListAddress != 0) || (destructorListAddress != 0))
            {
                CreateCallListInvokeCode(MemoryMarshal.Cast<byte, uint>(memory.GetMemorySpan((int)listRunAddress, (int)VMMemory.DataAlignment)));
            }

            // All code finalized, post-initialize the processor
            processor.PoolDatas = result;

            if (constructorListAddress != 0)
            {
                // Launch the constructor automatically
                processor.PostInitialize(listRunAddress);
                processor.Reg[Register.RA] = 0;
                processor.Reg[Register.P0] = constructorListAddress;
                processor.Reg[Register.P1] = ProgramStartOffset;
                processor.Reg[Register.PC] = listRunAddress;
            }
            else
            {
                // Launch the game code instead
                processor.PostInitialize(ProgramStartOffset);
                processor.Reg[Register.PC] = ProgramStartOffset;
            }

            processor.Reg[Register.SP] = stackStartAddress;
        }

        private void GetMetadataInfoAndSetupPersonalFolder(string inputFileName)
        {
            metaInfoReader = executable.GetMetaInfo();

            gameName = metaInfoReader?.Get("Title");

            if (metaInfoReader == null || gameName == null)
            {
                gameName = Path.ChangeExtension(Path.GetFileName(inputFileName), "");
                persistentDataPath = Path.Join(persistentDataPath, gameName);
            }
            else
            {
                persistentDataPath = Path.Join(persistentDataPath, gameName.ToValidFileName());
            }

            Directory.CreateDirectory(persistentDataPath);
        }

        public VMSystem(VMGPExecutable executable, VMSystemCreateParameters createParameters)
        {
            this.graphicDriver = createParameters.graphicDriver;
            this.inputDriver = createParameters.inputDriver;
            this.audioDriver = createParameters.audioDriver;
            this.timeDriver = createParameters.timeDriver;
            this.uiDriver = createParameters.uiDriver;
            this.persistentDataPath = createParameters.persistentDataPath;

            this.executable = executable;

            callMap = new VMCallMap(this);

            llvmCachePath = Path.Join(persistentDataPath, "__LLVMCache");
            Directory.CreateDirectory(llvmCachePath);

            GetMetadataInfoAndSetupPersonalFolder(createParameters.inputFileName);
        }

        public void PostInitialize()
        {
            VMLoader loader = new VMLoader(executable);

            // Make a gap after all program data to avoid weird stack manipulation
            uint totalSize = ProgramStartOffset + loader.EstimateNeededProgramSize() + VMMemory.DataAlignment;

            listRunAddress = totalSize;
            totalSize += VMMemory.DataAlignment;

            stackStartAddress = totalSize + executable.Header.stackSize;
            heapAddress = stackStartAddress + VMMemory.DataAlignment;       // Make a gap to avoid weird stack manipulation

            roundedHeapSize = MemoryUtil.AlignUp(executable.Header.dynamicDataHeapSize * 4 / 3, VMMemory.DataAlignment);
            totalSize = heapAddress + roundedHeapSize;

            memory = new VMMemory(totalSize);

            switch (GameSetting.cpuBackend)
            {
                case CPUBackend.LLVM:
                    processor = new PIP2.Translator.Translator(new PIP2.ProcessorConfig(),
                        gameName.ToValidFileName(),
                        memory, new TranslatorOptions()
                        {
                            cacheRootPath = llvmCachePath,
                            divideByZeroResultZero = true,
                            enableCache = true,
                            entryPoint = 0,
                            textBase = ProgramStartOffset
                        });

                    break;

                case CPUBackend.Interpreter:
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

                    break;
            }

            LoadModulesAndProgram(loader);
        }


        public void Stop()
        {
            shouldStop = true;
            processor.Stop();
        }

        public bool RunDestructor()
        {
            // Ignore all things and set pc directly
            if (destructorListAddress != 0)
            {
                processor.Reg[Register.P0] = destructorListAddress;
                processor.Reg[Register.PC] = listRunAddress;
                return true;
            }

            return false;
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

        public void Dispose()
        {
            VSoundModule.Dispose();
            VMusicModule.Dispose();
            VMStreamModule.Dispose();

            processor.Dispose();
        }

        public bool ShouldStop => shouldStop;
    }
}
