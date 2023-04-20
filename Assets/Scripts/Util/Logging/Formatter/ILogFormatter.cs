namespace Nofun.Util.Logging
{
    public interface ILogFormatter
    {
        string Format(LogEventArgs args);
    }
}