using Nofun.Util.Logging;
using UnityEngine;

namespace Nofun.Util.Unity
{
    public class UnityLogTarget : ILogTarget
    {
        private ILogFormatter formatter;

        public UnityLogTarget()
        {
            formatter = new UnityLogFormatter();
        }

        public string Name => "Unity";

        public void Log(object sender, LogEventArgs args)
        {
            string msg = formatter.Format(args);

            if (args.logLevel <= LogLevel.Debug)
            {
                Debug.Log(msg);
            }
            else if (args.logLevel == LogLevel.Warning)
            {
                Debug.LogWarning(msg);
            }
            else
            {
                Debug.LogError(msg);
            }
        }
    }
}