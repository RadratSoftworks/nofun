using Nofun.Util.Logging;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        [ModuleCall]
        public int vSysCtl(int cmd, int op)
        {
            Logger.Trace(LogClass.VMGPSystem, "System control stubbed!");
            return 1;
        }
    }
}