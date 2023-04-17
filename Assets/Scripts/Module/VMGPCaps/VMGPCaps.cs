using Nofun.VM;
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

            return 0;
        }

        [ModuleCall]
        private int vGetCaps(CapsQueryType queryType, VMPtr<Any> buffer)
        {
            switch (queryType)
            {
                case CapsQueryType.Video:
                    return GetCapsVideo(buffer.Cast<VideoCaps>());

                default:
                    throw new UnimplementedFeatureException("Other capabilities than video has not been implemented!");
            }
        }
    };
}