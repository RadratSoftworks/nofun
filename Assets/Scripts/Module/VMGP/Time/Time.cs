using Nofun.Driver.Time;
using Nofun.VM;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        [ModuleCall]
        private uint vGetTickCount()
        {
            return system.TimeDriver.GetMilliSecsTickCount();
        }

        [ModuleCall]
        private void vGetTimeDate(VMPtr<VMDateTime> dateTime)
        {
            var result = system.TimeDriver.GetDateTimeDetail(false);
            dateTime.Write(system.Memory, result);
        }

        [ModuleCall]
        private void vGetTimeDateUTC(VMPtr<VMDateTime> dateTime)
        {
            var result = system.TimeDriver.GetDateTimeDetail(true);
            dateTime.Write(system.Memory, result);
        }
    }
}