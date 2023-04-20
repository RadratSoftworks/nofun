namespace Nofun.PIP2.Encoding
{
    public struct RangeRegEncoding : IEncoding
    {
        public byte opcode;
        public byte start;
        public byte count;

        public uint Instruction
        {
            set
            {
                opcode = (byte)(value & 0xFF);
                start = (byte)((value >> 8) & 0xFF);
                count = (byte)((value >> 16) & 0xFF);

                if (count == 0)
                {
                    throw new InvalidPIP2EncodingException("The register range count can not be 0!");
                }
            }
        }
    }
}