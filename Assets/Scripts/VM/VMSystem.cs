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
using Nofun.Parser;
using Nofun.PIP2;
using System;
using Nofun.Util;
using Nofun.Driver.Time;
using System.IO;

namespace Nofun.VM
{
    public partial class VMSystem
    {
        private const uint ProgramStartOffset = VMMemory.DataAlignment;
        private const int InstructionPerRun = 10000;

        private VMMemory memory;
        private VMCallMap callMap;
        private VMMetaInfoReader metaInfoReader;
        private Processor processor;
        private IGraphicDriver graphicDriver;
        private IInputDriver inputDriver;
        private IAudioDriver audioDriver;
        private ITimeDriver timeDriver;
        private VMGPExecutable executable;
        private uint roundedHeapSize;
        private string persistentDataPath;

        private uint constructorAddress;
        private uint destructorAddress;
        private uint stackStartAddress;
        private uint heapAddress;

        private bool shouldStop = false;

        public VMMemory Memory => memory;
        public Processor Processor => processor;
        public IGraphicDriver GraphicDriver => graphicDriver;
        public IInputDriver InputDriver => inputDriver;
        public IAudioDriver AudioDriver => audioDriver;
        public ITimeDriver TimeDriver => timeDriver;
        public VMGPExecutable Executable => executable;
        public uint HeapStart => heapAddress;
        public uint HeapSize => roundedHeapSize;
        public uint HeapEnd => HeapStart + HeapSize;
        public string PersistentDataPath => persistentDataPath;

        private void LoadModulesAndProgram(VMLoader loader)
        {
            RegisterModules();

            var result = loader.Load(memory.GetMemorySpan((int)ProgramStartOffset, (int)loader.EstimateNeededProgramSize()),
                ProgramStartOffset, callMap);

            constructorAddress = result.Find(poolData => poolData?.Name?.Equals("~C", StringComparison.OrdinalIgnoreCase) ?? false)?.ImmediateInteger ?? 0;
            destructorAddress = result.Find(poolData => poolData?.Name?.Equals("~D", StringComparison.OrdinalIgnoreCase) ?? false)?.ImmediateInteger ?? 0;

            if (constructorAddress != 0)
            {
                constructorAddress = Memory.ReadMemory32(constructorAddress);
            }

            if (destructorAddress != 0)
            {
                destructorAddress = Memory.ReadMemory32(destructorAddress);
            }

            if (constructorAddress != 0)
            {
                // Launch the constructor automatically
                processor.Reg[Register.RA] = ProgramStartOffset;
                processor.Reg[Register.PC] = constructorAddress;
            }
            else
            {
                // Launch the game code instead
                processor.Reg[Register.PC] = ProgramStartOffset;
            }

            processor.Reg[Register.SP] = stackStartAddress;
            processor.PoolDatas = result;
        }

        private void GetMetadataInfoAndSetupPersonalFolder(string inputFileName)
        {
            Span<byte> magic = stackalloc byte[4];

            for (int i = 0; i < executable.ResourceCount; i++)
            {
                executable.ReadResourceData((uint)i, magic, 0);
                if (VMMetaInfoReader.IsMetadataMagic(magic))
                {
                    byte[] wholeMetadata = new byte[executable.GetResourceSize((uint)i)];
                    executable.ReadResourceData((uint)i, wholeMetadata, 0);

                    using (MemoryStream stream = new MemoryStream(wholeMetadata))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            metaInfoReader = new VMMetaInfoReader(reader);
                            break;
                        }
                    }
                }
            }

            string gameName = metaInfoReader?.Get("Title");

            if (metaInfoReader == null || gameName == null)
            {
                persistentDataPath = Path.Join(persistentDataPath, Path.ChangeExtension(Path.GetFileName(inputFileName), ""));
            }
            else
            {
                persistentDataPath = Path.Join(persistentDataPath, gameName.Replace(" ", ""));
            }

            Directory.CreateDirectory(persistentDataPath);
        }

        public VMSystem(VMGPExecutable executable, VMSystemCreateParameters createParameters)
        {
            this.graphicDriver = createParameters.graphicDriver;
            this.inputDriver = createParameters.inputDriver;
            this.audioDriver = createParameters.audioDriver;
            this.timeDriver = createParameters.timeDriver;
            this.persistentDataPath = createParameters.persistentDataPath;

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
            GetMetadataInfoAndSetupPersonalFolder(createParameters.inputFileName);
        }

        public void Stop()
        {
            shouldStop = true;
            processor.Stop();
        }

        public bool RunDestructor()
        {
            // Ignore all things and set pc directly
            if (destructorAddress != 0)
            {
                processor.Reg[Register.PC] = destructorAddress;
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

        public bool ShouldStop => shouldStop;
    }
}