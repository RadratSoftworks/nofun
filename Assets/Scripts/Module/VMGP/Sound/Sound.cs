/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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

            if (currentSound != null)
            {
                currentSound.Stop();
                currentSound = null;
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