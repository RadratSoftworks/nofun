using Nofun.VM;
using Nofun.Util.Allocator;
using Nofun.Util.Logging;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private ISpaceAllocator heapAllocator;

        [ModuleCall]
        private VMPtr<Any> vNewPtr(uint size)
        {
            long offset = heapAllocator.Allocate(size);
            if (offset < 0)
            {
                Logger.Error(LogClass.VMGPSystem, $"Failed to allocate new pointer with size of {size}");
                return VMPtr<Any>.Null;
            }

            return new VMPtr<Any>((uint)(offset + system.HeapStart));
        }

        [ModuleCall]
        private void vDisposePtr(VMPtr<Any> ptr)
        {
            if ((ptr < system.HeapStart) || (ptr >= system.HeapEnd))
            {
                Logger.Warning(LogClass.VMGPSystem, $"Pointer 0x{ptr.address:X} is not in heap range, dispose failed!");
            }
            else
            {
                heapAllocator.Free((long)ptr.address - system.HeapStart);
            }
        }
    }
}