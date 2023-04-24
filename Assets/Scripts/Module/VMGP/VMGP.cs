using Nofun.Util.Logging;
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
            this.tilemapCache = new();
            this.heapAllocator = new BlockAllocator(system.HeapSize);
        }

        public void OnSystemLoaded()
        {
        }

        [ModuleCall]
        private void DbgPrintf(VMString message)
        {
            // TODO: A mechanism to handle printf arguments
            Logger.Debug(LogClass.GameTTY, message.Get(system.Memory));
        }
    }
}