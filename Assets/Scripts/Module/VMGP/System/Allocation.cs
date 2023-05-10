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
using Nofun.Util.Allocator;
using Nofun.Util.Logging;

using System;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private ISpaceAllocator heapAllocator;

        [ModuleCall]
        private uint vMemFree()
        {
            return (uint)heapAllocator.AmountFree;
        }

        [ModuleCall]
        private VMPtr<Any> vNewPtr(uint size)
        {
            long offset = heapAllocator.Allocate(size);
            if (offset < 0)
            {
                Logger.Error(LogClass.VMGPSystem, $"Failed to allocate new pointer with size of {size}");
                return VMPtr<Any>.Null;
            }

            // We need to memset this to zero. If this is debug mode, memset it to 0x1C
            Span<byte> allocatedSpan = system.Memory.GetMemorySpan((int)(offset + system.HeapStart), (int)size);
            allocatedSpan.Fill(0);

            return new VMPtr<Any>((uint)(offset + system.HeapStart));
        }

        [ModuleCall]
        private void vDisposePtr(VMPtr<Any> ptr)
        {
            if (ptr.IsNull)
            {
                return;
            }

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