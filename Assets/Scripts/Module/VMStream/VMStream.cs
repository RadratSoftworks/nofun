using Nofun.VM;
using System;

namespace Nofun.Module.VMStream
{
    [Module]
    public partial class VMStream
    {
        private VMSystem system;
        private SimpleObjectManager<IVMHostStream> streams;

        public VMStream(VMSystem system)
        {
            this.system = system;
            streams = new();
        }

        [ModuleCall]
        private int vStreamOpen(VMString fileName, uint mode)
        {
            StreamType wantedType = (StreamType)(mode & 0xFF);
            IVMHostStream targetedStream;

            try
            {
                switch (wantedType)
                {
                    case StreamType.Resource:
                        {
                            targetedStream = InMemoryResourceStream.Create(system.Executable, mode);
                            break;
                        }
                    default:
                        {
                            throw new UnimplementedFeatureException($"Unimplemented stream type: {wantedType}!");
                        }
                }
            }
            catch (System.Exception e)
            {
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
        private void vStreamClose(int handle)
        {
            streams.Remove(handle);
        }
    };
}