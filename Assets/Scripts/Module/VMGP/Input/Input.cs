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

        [ModuleCall]
        private int vTestKey(uint key)
        {
            return system.InputDriver.KeyPressed(key) ? 1 : 0;
        }
    }
}