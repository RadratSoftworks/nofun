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

            SystemDeviceVendor vendor = SystemDeviceVendor.Unknown;

            switch (system.GameSetting.deviceModel)
            {
                case SystemDeviceModel.SonyEricssonT300:
                case SystemDeviceModel.SonyEricssonT310:
                case SystemDeviceModel.SonyEricssonT610:
                case SystemDeviceModel.SonyErricssonT226:
                case SystemDeviceModel.SonyErricssonP800:
                case SystemDeviceModel.SonyErricisonP900:
                    vendor = SystemDeviceVendor.SonyEricsson;
                    break;

                case SystemDeviceModel.Nokia3650:
                case SystemDeviceModel.Nokia6600:
                case SystemDeviceModel.Nokia7650:
                case SystemDeviceModel.NokiaNgage:
                    vendor = SystemDeviceVendor.Nokia;
                    break;

                case SystemDeviceModel.SendoX:
                    vendor = SystemDeviceVendor.Sendo;
                    break;

                case SystemDeviceModel.MotorolaA920:
                case SystemDeviceModel.MotorolaA925:
                    vendor = SystemDeviceVendor.Motorola;
                    break;

                case SystemDeviceModel.SiemensSX1:
                    vendor = SystemDeviceVendor.Siemens;
                    break;

                case SystemDeviceModel.TigerTelematicGametrac:
                    vendor = SystemDeviceVendor.TTPCom;
                    break;

                default:
                    vendor = SystemDeviceVendor.Unknown;
                    break;
            }

            capsAssign.deviceId = GetDeviceId(vendor, system.GameSetting.deviceModel);

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

        private int GetCapsComms(VMPtr<CommCaps> capsPtr)
        {
            CommCaps caps = new CommCaps()
            {
                size = (ushort)Marshal.SizeOf<CommCaps>(),
                flags = (ushort)CommCapsFlags.StandardMorden
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

                case CapsQueryType.Communication:
                    return GetCapsComms(buffer.Cast<CommCaps>());

                default:
                    throw new UnimplementedFeatureException($"Unimplemented capability {queryType}!");
            }
        }
    };
}