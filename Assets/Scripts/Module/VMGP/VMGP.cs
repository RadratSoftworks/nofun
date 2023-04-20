using Nofun.Driver.Graphics;
using Nofun.Util.Allocator;
using Nofun.VM;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private VM.VMSystem system;

        public VMGP(VM.VMSystem system)
        {
            this.system = system;
            this.fontCache = new();
            this.spriteCache = new();
            this.heapAllocator = new BlockAllocator(system.HeapSize);
        }

        public void OnSystemLoaded()
        {
        }
    }
}