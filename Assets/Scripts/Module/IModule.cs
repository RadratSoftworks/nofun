using Nofun.VM;

namespace Nofun.Module
{
    public interface IModule
    {
        void Register(VMCallMap callMap);
    }
}