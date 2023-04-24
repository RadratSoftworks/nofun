/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace Nofun.Util.Logging
{
    public static class Logger
    {
        private static LogLevel minLevel = LogLevel.Debug;
        private static event EventHandler<LogEventArgs> Outputter;
        private static List<ILogTarget> logTargets = new();

        private static void DoTryLog(LogLevel targetLevel, LogClass logClass, string message)
        {
            if (minLevel > targetLevel)
            {
                return;
            }

            Outputter?.Invoke(null, new LogEventArgs(DateTime.Now, targetLevel, logClass, message));
        }

        public static void AddTarget(ILogTarget target)
        {
            if (!logTargets.Contains(target))
            {
                logTargets.Add(target);
                Outputter += target.Log;
            }
        }

        public static void RemoveTarget(ILogTarget target)
        {
            if (logTargets.Contains(target))
            {
                Outputter -= target.Log;
                logTargets.Remove(target);
            }
        }

        public static void Trace(LogClass logClass, string message) => DoTryLog(LogLevel.Trace, logClass, message);
        public static void Debug(LogClass logClass, string message) => DoTryLog(LogLevel.Debug, logClass, message);
        public static void Warning(LogClass logClass, string message) => DoTryLog(LogLevel.Warning, logClass, message);
        public static void Error(LogClass logClass, string message) => DoTryLog(LogLevel.Error, logClass, message);
        public static void Fatal(LogClass logClass, string message) => DoTryLog(LogLevel.Fatal, logClass, message);
    };
}