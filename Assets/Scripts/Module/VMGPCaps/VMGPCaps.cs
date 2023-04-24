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

using Nofun.Util.Logging;
using Nofun.VM;
using System;
using System.Runtime.InteropServices;

namespace Nofun.Module.VMGPCaps
{
    [Module]
    public partial class VMGPCaps
    {
        private VMSystem system;

        public VMGPCaps(VMSystem system)
        {
            this.system = system;
        }

        private int GetCapsVideo(VMPtr<VideoCaps> caps)
        {
            VideoCaps capsAssign = new VideoCaps();

            capsAssign.width = (ushort)system.GraphicDriver.ScreenWidth;
            capsAssign.height = (ushort)system.GraphicDriver.ScreenHeight;
            capsAssign.size = (ushort)Marshal.SizeOf<VideoCaps>();
            capsAssign.flags = (ushort)VideoCapsFlag.All;

            caps.Write(system.Memory, capsAssign);

            return 1;
        }

        private uint GetDeviceId(SystemDeviceVendor vendor, SystemDeviceModel model)
        {
            return ((uint)model << 16) | (uint)vendor;
        }

        private int GetCapsSystem(VMPtr<SystemCaps> caps)
        {
            SystemCaps capsAssign = new SystemCaps();

            capsAssign.size = (ushort)Marshal.SizeOf<SystemCaps>();
            capsAssign.flags = (ushort)SystemCapsFlags.MordenDeviceCapsExceptEndian;

            if (!BitConverter.IsLittleEndian)
            {
                capsAssign.flags |= (ushort)SystemCapsFlags.BigEndian;
            }

            Logger.Trace(LogClass.VMGPCaps, "System capabilities device ID stubbed with Nokia N-Gage");

            capsAssign.deviceId = GetDeviceId(SystemDeviceVendor.Nokia, SystemDeviceModel.NokiaNgage);

            caps.Write(system.Memory, capsAssign);
            return 1;
        }

        private int GetCapsSound(VMPtr<SoundCaps> capsPtr)
        {
            SoundCaps caps = new SoundCaps()
            {
                size = (ushort)Marshal.SizeOf<SoundCaps>(),
                flags = (ushort)system.AudioDriver.Capabilities,
                config = system.AudioDriver.SoundConfig
            };

            capsPtr.Write(system.Memory, caps);
            return 1;
        }

        [ModuleCall]
        private int vGetCaps(CapsQueryType queryType, VMPtr<Any> buffer)
        {
            switch (queryType)
            {
                case CapsQueryType.Video:
                    return GetCapsVideo(buffer.Cast<VideoCaps>());

                case CapsQueryType.System:
                    return GetCapsSystem(buffer.Cast<SystemCaps>());

                case CapsQueryType.Sound:
                    return GetCapsSound(buffer.Cast<SoundCaps>());

                default:
                    throw new UnimplementedFeatureException($"Unimplemented capability {queryType}!");
            }
        }
    };
}