using Nofun.VM;
using Nofun.Driver.Audio;
using System;
using Nofun.Util.Logging;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private ISound currentSound;

        [ModuleCall]
        private int vPlayResource(VMPtr<byte> data, uint length, uint flags)
        {
            if ((flags & (uint)SoundFlag.Stop) != 0)
            {
                currentSound?.Stop();
                return 1;
            }

            SoundType soundType = (SoundType)(flags & 0xF);
            bool loop = (((flags & (uint)SoundFlag.Loop) != 0) ? true : false);

            Span<byte> dataRead;

            if ((flags & (uint)SoundFlag.Stream) != 0)
            {
                int streamHandle = (int)data.address;
                VMStream.IVMHostStream stream = system.VMStreamModule.GetStream(streamHandle);

                if (stream == null)
                {
                    Logger.Error(LogClass.VMGPSound, $"No stream with handle {streamHandle} found, resource play failed!");
                    return 0;
                }

                dataRead = new byte[length];
                if (stream.Read(dataRead, null) != length)
                {
                    Logger.Error(LogClass.VMGPSound, $"Failed to to read {length} bytes resource data from stream, resource play failed!");
                    return 0;
                }
            }
            else
            {
                dataRead = data.AsSpan(system.Memory, (int)length);
            }

            currentSound = system.AudioDriver.PlaySound(soundType, dataRead, loop);
            return 1;
        }
    }
}