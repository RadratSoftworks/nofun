using Nofun.PIP2;

namespace Nofun.Module.VMGP
{
    public class TaskInfo
    {
        public uint entryPoint = 0;
        public uint stackAddress = 0;

        public int taskID = 0;
        public uint[] taskContext = new uint[Register.TotalReg];

        public int receivedData = 0;
    }
}