namespace Nofun.PIP2.Encoding
{
    public struct TwoSourcesEncoding : IEncoding
    {
        public byte opcode;
        public byte d;
        public byte s;
        public byte t;

        public uint Instruction
        {
            set
            {
                opcode = (byte)(value & 0xFF);
                d = (byte)(((value >> 8) & 0xFF) >> IEncoding.RegisterRealIndexShift);
                s = (byte)(((value >> 16) & 0xFF) >> IEncoding.RegisterRealIndexShift);
                t = (byte)(((value >> 24) & 0xFF) >> IEncoding.RegisterRealIndexShift);
            }
        }
    }
}