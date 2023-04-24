using Nofun.VM;
using System;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        [ModuleCall]
        private VMPtr<byte> vStrCpy(VMPtr<byte> dest, VMPtr<byte> source)
        {
            // Sometimes it's also used like memcpy, so we must do byte-by-byte
            while (true)
            {
                byte sourceByte = source.Read(system.Memory);
                dest.Write(system.Memory, sourceByte);

                if (sourceByte == 0)
                {
                    break;
                }

                source += 1;
                dest += 1;
            }

            return dest;
        }

        [ModuleCall]
        private int vStrLen(VMString str)
        {
            return str.Get(system.Memory).Length;
        }

        private VMPtr<byte> NumberToString(long val, VMPtr<byte> buf, byte len, byte pad)
        {
            string valConverted = val.ToString();
            Span<byte> destBuf = buf.AsSpan(system.Memory, Math.Max(valConverted.Length + 1, len + 1));

            for (int i = 0; i < valConverted.Length; i++)
            {
                destBuf[i] = (byte)valConverted[i];
            }

            if (valConverted.Length < len)
            {
                for (int i = valConverted.Length; i < len; i++)
                {
                    destBuf[i] = pad;
                }
            }

            destBuf[Math.Max(valConverted.Length, len)] = 0;
            return buf + len;
        }

        [ModuleCall]
        private VMPtr<byte> vitoa(int val, VMPtr<byte> buf, byte len, byte pad)
        {
            return NumberToString(val, buf, len, pad);
        }

        [ModuleCall]
        private VMPtr<byte> vutoa(uint val, VMPtr<byte> buf, byte len, byte pad)
        {
            return NumberToString(val, buf, len, pad);
        }
    }
}