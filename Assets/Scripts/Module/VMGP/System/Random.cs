using System;

namespace Nofun.Module.VMGP
{
    [Module]
    public partial class VMGP
    {
        private Random randomizer = null;
        private uint previousSeed = 0;

        private const int RandomMax = 0xFFFF;

        [ModuleCall]
        private void vSetRandom(uint seed)
        {
            if ((randomizer == null) || (previousSeed != seed))
            {
                previousSeed = seed;
                randomizer = new Random((int)seed);
            }
        }

        [ModuleCall]
        private uint vGetRandom()
        {
            if (randomizer == null)
            {
                randomizer = new Random();
            }

            return (uint)randomizer.Next(0, RandomMax);
        }
    }
}