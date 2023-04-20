using System;
using Nofun.PIP2.Encoding;

namespace Nofun.PIP2.Interpreter
{
    public partial class Interpreter : Processor
    {
        private Action<UInt32>[] OpcodeTables;

        private bool shouldStop = false;
        private bool isRunning = false;
        private int instructionRan = 0;

        public override int InstructionRan => instructionRan;

        public Interpreter(ProcessorConfig config) : base(config)
        {
            OpcodeTables = new Action<UInt32>[116]
            {
                null, null, I(ADD), I(AND), I(MUL), I(DIV), I(DIVU), null,    // 0x00
                null, I(SUB), null, null, null, null, null, I(EXSB),    // 0x08
                I(EXSH), I(MOV), I(ADDB), null, I(ANDB), null, I(MOVB), I(ADDH),    // 0x10
                null, null, null, I(MOVH), I(SLLi), I(SRAi), I(SRLi), I(ADDQ),    // 0x18
                I(MULQ), I(ADDBi), I(ANDBi), I(ORBi), null, I(SRLB), null, I(ADDHi),    // 0x20
                null, I(SLLH), I(SRAH), null, I(BEQI), I(BNEI), I(BGEI), null,    // 0x28
                I(BGTI), I(BGTUI), I(BLEI), I(BLEUI), I(BLTI), I(BLTUI), I(BEQIB), I(BNEIB),    // 0x30
                I(BGEIB), null, I(BGTIB), I(BGTUIB), I(BLEIB), I(BLEUIB), I(BLTIB), I(BLTUIB),    // 0x38
                I(LDQ), I(JPr), null, I(STORE), I(RESTORE), I(RET), null, null,    // 0x40
                I(SYSCPY), I(SYSSET), I(ADDi), I(ANDi), null, I(DIVi), I(DIVUi), null,    // 0x48
                null, I(SUBi), I(STBd), I(STHd), I(STWd), I(LDBd), null, I(LDWd),    // 0x50
                I(LDBud), I(LDHu), I(LDI), I(JPl), I(CALLl), I(BEQ), I(BNE), I(BGE),    // 0x58
                I(BGEU), I(BGT), I(BGTU), I(BLE), I(BLEU), I(BLT), I(BLTU), null,    // 0x60
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

        public override void Run(int instructionPerRun)
        {
            if (isRunning)
            {
                throw new InvalidOperationException("Can't call Run() when is already running.");
            }

            shouldStop = false;
            isRunning = true;

            instructionRan = 0;

            while (!shouldStop && (instructionRan < instructionPerRun))
            {
                uint value = config.ReadCode(registers[Register.PCIndex]);
                Action<UInt32> handler = OpcodeTables[value & 0xFF];

                if (handler == null)
                {
                    throw new InvalidOperationException("Unimplemented opcode " + ((Opcode)(value & 0xFF)).ToString());
                }

                registers[Register.PCIndex] += InstructionSize;
                handler(value);

                instructionRan++;
            }

            isRunning = false;
        }

        public override void Stop()
        {
            shouldStop = true;
        }
    }
}