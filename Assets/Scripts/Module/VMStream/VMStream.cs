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

using Nofun.Module.VMGP;
using Nofun.Util.Logging;
using Nofun.VM;
using System;

namespace Nofun.Module.VMStream
{
    [Module]
    public partial class VMStream
    {
        private VMSystem system;
        private SimpleObjectManager<IVMHostStream> streams;

        public IVMHostStream GetStream(int handle)
        {
            return streams.Get(handle);
        }

        public VMStream(VMSystem system)
        {
            this.system = system;
            streams = new();
        }

        public IVMHostStream Open(string fileName, uint mode)
        {
            StreamType wantedType = (StreamType)(mode & 0xFF);
            IVMHostStream targetedStream;

            switch (wantedType)
            {
                case StreamType.Resource:
                    {
                        targetedStream = VMResourceStream.Create(system.Executable, mode);
                        break;
                    }
                case StreamType.File:
                    {
                        targetedStream = VMWrapperIStream.Create(system.PersistentDataPath, fileName, mode);
                        break;
                    }
                default:
                    {
                        throw new UnimplementedFeatureException($"Unimplemented stream type: {wantedType}!");
                    }
            }

            return targetedStream;
        }

        [ModuleCall]
        private int vStreamOpen(VMString fileName, uint mode)
        {
            IVMHostStream targetedStream;

            try
            {
                targetedStream = Open((fileName.Address == 0) ? "" : fileName.Get(system.Memory), mode);
            }
            catch (System.Exception e)
            {
                Logger.Error(LogClass.VMStream, $"Stream open failed: {e}");
                return -1;
            }

            return streams.Add(targetedStream);
        }

        [ModuleCall]
        private int vStreamRead(int handle, VMPtr<byte> buffer, int count)
        {
            IVMHostStream stream = streams.Get(handle);
            if (stream == null)
            {
                return -1;
            }

            Span<byte> bufferSpan = buffer.AsSpan(system.Memory, count);
            return stream.Read(bufferSpan, null);
        }

        [ModuleCall]
        private int vStreamWrite(int handle, VMPtr<byte> buffer, int count)
        {
            IVMHostStream stream = streams.Get(handle);
            if (stream == null)
            {
                return -1;
            }

            Span<byte> bufferSpan = buffer.AsSpan(system.Memory, count);
            return stream.Write(bufferSpan, null);
        }


        [ModuleCall]
        private int vStreamSeek(int handle, int where, StreamSeekMode whence)
        {
            IVMHostStream stream = streams.Get(handle);
            if (stream == null)
            {
                return -1;
            }

            return stream.Seek(where, whence);
        }

        [ModuleCall]
        private void vStreamClose(int handle)
        {
            IVMHostStream stream = streams.Get(handle);
            if (stream != null)
            {
                stream.OnClose();
                streams.Remove(handle);
            }
        }
    };
}