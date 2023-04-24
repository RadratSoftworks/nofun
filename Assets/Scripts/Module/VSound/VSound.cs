using Nofun.VM;

namespace Nofun.Module.VSound
{
    [Module]
    public partial class VSound
    {
        private const int SND_OK = 0;
        private const int SND_ERR = -1;

        private VMSystem system;

        public VSound(VMSystem system)
        {
            this.system = system;
        }

        [ModuleCall]
        int vSoundInit()
        {
            if (!system.AudioDriver.InitializePCMPlay())
            {
                return SND_ERR;
            }

            return SND_OK;
        } 
    }
}