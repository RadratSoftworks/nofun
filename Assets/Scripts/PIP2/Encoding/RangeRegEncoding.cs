namespace Nofun.PIP2.Encoding
{
    public struct RangeRegEncoding : IEncoding
    {
        public byte opcode;
        public byte d;
        public byte s;

        public uint Instruction
        {
            set
            {
                opcode = (byte)(value & 0xFF);
                d = (byte)((value >> 8) & 0xFF);
                s = (byte)((value >> 16) & 0xFF);

                if (s == 0)
                {
                    throw new InvalidPIP2EncodingException("The register range count can not be 0!");
                }

                d = (byte)(d + s - 1);
            }
        }
    }
}