using System;
using Nofun.PIP2.Encoding;
using Unity.VisualScripting;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private Action<UInt32>[] OpcodeTables;

        private bool shouldStop = false;
        private bool isRunning = false;

        public Interpreter(ProcessorConfig config) : base(config)
        {
            OpcodeTables = new Action<UInt32>[116]
            {
                null, null, null, null, null, null, null, null,    // 0x00
                null, I(SUB), null, null, null, null, null, null,    // 0x08
                null, null, null, null, null, null, null, null,    // 0x10
                null, null, null, null, null, null, null, I(ADDQ),    // 0x18
                null, null, null, null, null, null, null, I(ADDHi),    // 0x20
                null, null, I(SRAH), null, null, null, null, null,    // 0x28
                null, null, null, null, null, null, null, null,    // 0x30
                null, null, null, null, null, null, null, null,    // 0x38
                I(LDQ), null, null, I(STORE), I(RESTORE), null, null, null,    // 0x40
                null, null, null, null, null, null, null, null,    // 0x48
                null, null, null, I(STHd), null, null, null, null,    // 0x50
                null, I(LDHu), I(LDI), null, I(CALLl), null, null, null,    // 0x58
                null, null, null, null, null, null, null, null,    // 0x60
                null, null, null, null, null, null, null, null,    // 0x68
                null, null, null, null                             // 0x70
            };
        }

        #region Instruction Handler Wrapper
        private Action<UInt32> InstructionWrapper<T>(Action<T> handler) where T : IEncoding, new()
        {
            return value => handler(new T()
            {
                Instruction = value
            });
        }

        private Action<UInt32> I(Action<TwoSourcesEncoding> handler)
        {
            return InstructionWrapper<TwoSourcesEncoding>(handler);
        }

        private Action<UInt32> I(Action<UndefinedEncoding> handler)
        {
            return InstructionWrapper<UndefinedEncoding>(handler);
        }

        private Action<UInt32> I(Action<DestOnlyEncoding> handler)
        {
            return InstructionWrapper<DestOnlyEncoding>(handler);
        }

        private Action<UInt32> I(Action<WordEncoding> handler)
        {
            return InstructionWrapper<WordEncoding>(handler);
        }
        private Action<UInt32> I(Action<RangeRegEncoding> handler)
        {
            return InstructionWrapper<RangeRegEncoding>(handler);
        }
        #endregion

        public override void Run()
        {
            if (isRunning)
            {
                throw new InvalidOperationException("Can't call Run() when is already running.");
            }

            shouldStop = false;
            isRunning = true;

            while (!shouldStop)
            {
                uint value = config.ReadCode(registers[Register.PCIndex]);
                Action<UInt32> handler = OpcodeTables[value & 0xFF];

                if (handler == null)
                {
                    throw new InvalidOperationException("Unimplemented opcode " + ((Opcode)(value & 0xFF)).ToString());
                }

                registers[Register.PCIndex] += InstructionSize;
                handler(value);
            }

            isRunning = false;
        }

        public override void Stop()
        {
            shouldStop = true;
        }
    }
}