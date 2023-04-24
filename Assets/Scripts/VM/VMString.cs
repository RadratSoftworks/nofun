using System;
using System.Runtime.InteropServices;

namespace Nofun.VM
{
    public struct VMString
    {
        private UInt32 address;

        public VMString(uint address)
        {
            this.address = address;
        }

        public uint Address => address;

        public string Get(VMMemory memory, bool isUtf16 = false)
        {
            string value = "";
            uint curAddr = address;

            do
            {
                char val = '\0';
                if (isUtf16)
                {
                    val = (char)memory.ReadMemory16(curAddr);
                    curAddr += 2;
                } else
                {
                    val = (char)memory.ReadMemory8(curAddr++);
                }
                if (val == '\0')
                {
                    break;
                }
                value += val;
            } while (true);

            return value;
        }

        public void Set(VMMemory memory, string value)
        {
            var destSpan = memory.GetMemorySpan((int)address, value.Length + 1);
            value.AsSpan().CopyTo(MemoryMarshal.Cast<byte, char>(destSpan));

            destSpan[value.Length] = 0;
        }
    }
}