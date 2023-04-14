using System;

namespace Nofun.PIP2
{
    public class ProcessorConfig
    {
        public Func<UInt32, UInt32> ReadCode;

        public Func<UInt32, byte> ReadByte;
        public Func<UInt32, UInt16> ReadWord;
        public Func<UInt32, UInt32> ReadDword;

        public Action<UInt32, byte> WriteByte;
        public Action<UInt32, UInt16> WriteWord;
        public Action<UInt32, UInt32> WriteDword;
    }
}