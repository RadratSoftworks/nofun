using System;

namespace Nofun.Util.Logging
{
    public class LogEventArgs : EventArgs
    {
        public readonly DateTime time;
        public readonly LogLevel logLevel;
        public readonly LogClass logClass;
        public readonly string message;

        public LogEventArgs(DateTime time, LogLevel logLevel, LogClass logClass, string message)
        {
            this.time = time;
            this.logLevel = logLevel;
            this.logClass = logClass;
            this.message = message;
        }
    }
}