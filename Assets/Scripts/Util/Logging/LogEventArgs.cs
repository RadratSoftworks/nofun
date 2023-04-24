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