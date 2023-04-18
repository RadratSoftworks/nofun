using Nofun.VM;

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
    };
}