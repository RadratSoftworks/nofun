namespace Nofun.PIP2.Encoding
{
    public struct DestOnlyEncoding : IEncoding
    {
        public byte opcode;
        public byte d;

        public uint Instruction
        {
            set
            {
                opcode = (byte)(value & 0xFF);
                d = (byte)(((value >> 8) & 0xFF) >> IEncoding.RegisterRealIndexShift);
            }
        }
    }
}