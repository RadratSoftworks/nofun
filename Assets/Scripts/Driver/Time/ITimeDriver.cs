using Nofun.Module.VMGP;

namespace Nofun.Driver.Time
{
    public interface ITimeDriver
    {
        uint GetMilliSecsTickCount();

        long GetDateTime();

        VMDateTime GetDateTimeDetail(bool isUtc);
    }
}