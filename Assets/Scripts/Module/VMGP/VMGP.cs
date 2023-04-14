using Nofun.Driver.Graphics;
using Nofun.VM;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        VM.VMSystem system;

        public VMGP(VM.VMSystem system)
        {
            InitializePalette();

            this.system = system;
            this.fontCache = new();
        }
    }
}