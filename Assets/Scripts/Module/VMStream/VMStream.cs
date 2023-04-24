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
                            targetedStream = VMResourceStream.Create(system.Executable, mode);
                            break;
                        }
                    case StreamType.File:
                        {
                            targetedStream = VMWrapperIStream.Create(system.PersistentDataPath, fileName.Get(system.Memory), mode);
                            break;
                        }
                    default:
                        {
                            string filePath = fileName.Get(system.Memory);
                            throw new UnimplementedFeatureException($"Unimplemented stream type: {wantedType}!");
                        }
                }
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
            streams.Remove(handle);
        }
    };
}