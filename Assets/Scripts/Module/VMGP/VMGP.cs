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
using Nofun.Util.Allocator;
using Nofun.VM;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private const ushort MajorAPIVersion = 2;
        private const ushort MinorAPIVersion = 50;

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

        [ModuleCall]
        private uint vGetVMGPInfo()
        {
            return ((uint)MajorAPIVersion << 16) | MinorAPIVersion;
        }

        [ModuleCall]
        private uint vUID()
        {
            return 0xDEADBEEF;
        }
    }
}