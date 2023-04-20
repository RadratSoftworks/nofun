namespace Nofun.Util.Logging
{
    public class UnityLogFormatter : ILogFormatter
    {
        public string Format(LogEventArgs args)
        {
            return $"{args.logClass}: {args.message}";
        }
    }
}