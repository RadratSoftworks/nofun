namespace Nofun.Util.Logging
{
    public interface ILogTarget
    {
        public void Log(object sender, LogEventArgs args);
        public string Name { get; }
    }
}