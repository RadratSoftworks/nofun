namespace Nofun.Util.Logging
{
    public class DefaultLogFormatter : ILogFormatter
    {
        public string Format(LogEventArgs args)
        {
            return $" {args.logLevel.ToString()[0]} [{args.time.Millisecond / 1000.0f}] {args.logClass}: {args.message}";
        }
    }
}