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