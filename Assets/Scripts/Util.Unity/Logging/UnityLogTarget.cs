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