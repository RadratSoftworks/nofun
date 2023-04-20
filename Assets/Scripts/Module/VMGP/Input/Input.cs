namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        [ModuleCall]
        private uint vGetButtonData()
        {
            return system.InputDriver.GetButtonData();
        }
    }
}