using Nofun.VM;

namespace Nofun.Module.VMGP3D
{
    [Module]
    public partial class VMGP3D
    {
        private VMSystem system;

        public VMGP3D(VMSystem system)
        {
            this.system = system;
        }

        [ModuleCall]
        private void vInit3D()
        {
        }

        [ModuleCall]
        private void vSetViewport(int left, int top, int width, int height)
        {
            system.GraphicDriver.SetViewport(left, top, width, height);
        }
    }
}