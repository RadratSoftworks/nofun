using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using Nofun.Util.Logging;
using Nofun.VM;

namespace Nofun.PIP2.Translator
{
    public class Translator : Processor, Processor.IRegIndexer, IDisposable, Processor.IReg16Indexer, Processor.IReg8Indexer
    {
        enum ExceptionCode
        {
            NotCompiledFunction = 1
        }

        private delegate void HleHandler(IntPtr userData, int hleNum);

        private IntPtr enginePtr;
        private GCHandle pinnedMemoryHandle;
        private IntPtr poolItemsPtr;
        private HleHandler hleHandler;
        private Thread runThread;
        private string moduleName;

        private TranslatorOptions translatorOptions;
        private VMMemory memory;
        private bool notWorking;

        private void HandleHleCall(IntPtr userData, int num)
        {
            if (num < 0)
            {
                if (Enum.IsDefined(typeof(ExceptionCode), -num))
                {
                    ExceptionCode code = (ExceptionCode)(-num);
                    switch (code)
                    {
                        case ExceptionCode.NotCompiledFunction:
                            throw new InvalidOperationException($"Not compiled function called! PC={Reg[Register.PC]:X8}");
                    }
                }
            }

            PoolData poolData = GetPoolData((uint)num);
            if (poolData.DataType != PoolDataType.Import)
            {
                throw new InvalidOperationException("HLE call is not an import!");
            }

            poolData.Function();
        }

        public Translator(ProcessorConfig config, string moduleName, VMMemory memory, TranslatorOptions options) : base(config)
        {
            hleHandler = HandleHleCall;

            this.translatorOptions = options;
            this.moduleName = moduleName;
            this.memory = memory;
        }

        public override void PostInitialize(uint entryPoint)
        {
            pinnedMemoryHandle = GCHandle.Alloc(memory.memory, GCHandleType.Pinned);

            // Build pool items
            long[] poolItems = new long[poolDatas.Count];
            for (int i = 0; i < poolItems.Length; i++)
            {
                if (poolDatas[i].DataType == PoolDataType.Import || poolDatas[i].DataType == PoolDataType.None)
                {
                    if (poolDatas[i].Name == "vTerminateVMGP")
                    {
                        poolItems[i] = unchecked((long)0x8000000100000000);
                    }
                    else
                    {
                        poolItems[i] = unchecked((long)0x8000000000000000);
                    }
                }
                else
                {
                    poolItems[i] = unchecked(poolDatas[i].DataType == PoolDataType.ImmInteger
                        ? poolDatas[i].ImmediateInteger!.Value
                        : (uint)BitConverter.SingleToInt32Bits(poolDatas[i].ImmediateFloat!.Value));

                    if (poolDatas[i].Name.Equals("~C", StringComparison.OrdinalIgnoreCase) || poolDatas[i].Name.Equals("~D", StringComparison.OrdinalIgnoreCase))
                    {
                        poolItems[i] |= 0x2000000000000000;
                    }
                    else if (poolDatas[i].IsInCode)
                    {
                        poolItems[i] |= 0x4000000000000000;
                    }
                    else if (poolDatas[i].IsCodePointerRelocatedInData)
                    {
                        poolItems[i] |= 0x1000000000000000;
                    }
                }
            }

            poolItemsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)) * poolItems.Length);
            Marshal.Copy(poolItems, 0, poolItemsPtr, poolItems.Length);

            TranslatorConfig translatorConfig = new TranslatorConfig()
            {
                memoryBase = pinnedMemoryHandle.AddrOfPinnedObject(),
                memorySize = (ulong)memory.MemorySize,
                poolItemsBase = poolItemsPtr,
                poolItemsCount = (ulong)poolItems.Length
            };

            translatorOptions.entryPoint = entryPoint - translatorOptions.textBase;

            IntPtr optionsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(translatorOptions));
            Marshal.StructureToPtr(translatorOptions, optionsPtr, false);

            IntPtr configPtr = Marshal.AllocHGlobal(Marshal.SizeOf(translatorConfig));
            Marshal.StructureToPtr(translatorConfig, configPtr, false);

            enginePtr = TranslatorAPI.EngineCreate(moduleName, configPtr, optionsPtr);

            Marshal.FreeHGlobal(configPtr);
            Marshal.FreeHGlobal(optionsPtr);
        }

        public override void Dispose()
        {
            if (enginePtr != IntPtr.Zero)
            {
                TranslatorAPI.EngineDestroy(enginePtr);
                enginePtr = IntPtr.Zero;
            }

            if (pinnedMemoryHandle.IsAllocated)
            {
                pinnedMemoryHandle.Free();
            }

            if (poolItemsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(poolItemsPtr);
                poolItemsPtr = IntPtr.Zero;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public override void Run(int instructionPerRun)
        {
            if (enginePtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Engine is not created yet.");
            }

            if (runThread != null)
            {
                runThread.Abort();
            }

            if (notWorking)
            {
                return;
            }

            AutoResetEvent finishEvent = new(false);

            runThread = new Thread(() =>
            {
                try
                {
                    TranslatorAPI.EngineExecute(enginePtr, Marshal.GetFunctionPointerForDelegate(hleHandler),
                        IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    Logger.Error(LogClass.PIP2, $"Exception thrown while running translator, PC={Reg[Register.PC]:x8}. Details: {ex}");
                    notWorking = true;
                }

                finishEvent.Set();
            });

            runThread.Start();
            finishEvent.WaitOne();
        }

        public override void Stop()
        {
            if (enginePtr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Engine is not created yet.");
            }

            if (runThread != null)
            {
                runThread.Abort();
                runThread = null;
            }
        }

        uint IRegIndexer.this[uint index]
        {
            get
            {
                if (enginePtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Engine is not created yet.");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException("Register index must be multiple of 4.");
                }

                return TranslatorAPI.EngineGetRegister(enginePtr, (int)index);
            }
            set
            {
                if (enginePtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Engine is not created yet.");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException("Register index must be multiple of 4.");
                }

                TranslatorAPI.EngineSetRegister(enginePtr, (int)index, value);
            }
        }

        ushort IReg16Indexer.this[uint index]
        {
            get
            {
                if (enginePtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Engine is not created yet.");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException("Register index must be multiple of 4.");
                }

                return (ushort)TranslatorAPI.EngineGetRegister(enginePtr, (int)index);
            }
            set
            {
                if (enginePtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Engine is not created yet.");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException("Register index must be multiple of 4.");
                }

                TranslatorAPI.EngineSetRegister(enginePtr, (int)index, value);
            }
        }

        byte IReg8Indexer.this[uint index]
        {
            get
            {
                if (enginePtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Engine is not created yet.");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException("Register index must be multiple of 4.");
                }

                return (byte)TranslatorAPI.EngineGetRegister(enginePtr, (int)index);
            }
            set
            {
                if (enginePtr == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Engine is not created yet.");
                }

                if ((index & 3) != 0)
                {
                    throw new InvalidOperationException("Register index must be multiple of 4.");
                }

                TranslatorAPI.EngineSetRegister(enginePtr, (int)index, value);
            }
        }

        public override IRegIndexer Reg => this;
        public override IReg16Indexer Reg16 => this;
        public override IReg8Indexer Reg8 => this;


        public override ProcessorContext SaveContext()
        {
            ProcessorContext context = new ProcessorContext(new uint[Register.TotalReg]);

            for (int i = 0; i < Register.TotalReg; i++)
            {
                context.registers[i] = Reg[(uint)i * 4];
            }

            return context;
        }

        public override void LoadContext(ProcessorContext context)
        {
            for (int i = 0; i < Register.TotalReg; i++)
            {
                Reg[(uint)i * 4] = context.registers[i];
            }
        }

        public override int InstructionRan => 0;
    }
}
