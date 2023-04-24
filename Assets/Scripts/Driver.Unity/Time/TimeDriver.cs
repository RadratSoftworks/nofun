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

using Nofun.Driver.Time;
using Nofun.Module.VMGP;
using System;

namespace Nofun.Driver.Unity.Time
{
    public class TimeDriver : ITimeDriver
    {
        private readonly DateTime EpochDatetime = new DateTime(1970, 1, 1);
        private float timePassedSecs = 0.0f;

        private const float millisecsPerSec = 1000.0f;

        public long GetDateTime()
        {
            return (long)(DateTime.Now - EpochDatetime).TotalSeconds;
        }

        public VMDateTime GetDateTimeDetail(bool isUtc)
        {
            DateTime currentDt = isUtc ? DateTime.UtcNow : DateTime.Now;

            VMDateTime returnResult = new VMDateTime()
            {
                day = (ushort)currentDt.Day,
                year = (ushort)currentDt.Year,
                month = (byte)currentDt.Month,
                hour = (byte)currentDt.Hour,
                minute = (byte)currentDt.Minute,
                second = (byte)currentDt.Second,
            };

            return returnResult;
        }

        public uint GetMilliSecsTickCount()
        {
            return (uint)(timePassedSecs * millisecsPerSec);
        }

        public void Update()
        {
            timePassedSecs += UnityEngine.Time.deltaTime;
        }
    }
}