using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace SanyoniLib.SystemHelper
{

    public static class TimeHelper
    {

        public static long EpochTimeMillis
        {
            get { return GetEpochTimeMillis(); }
        }

        public static double EpochTimeSeconds
        {
            get { return EpochTimeSeconds; }
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long GetEpochTimeMillis()
        {
            // 10000 Tick당 1 Millisecond 이다.
            return (DateTime.Now.Ticks - epoch.Ticks) / 10000;
        }

        private static double GetEpochTimeSeconds()
        {
            long epochMillis = GetEpochTimeMillis();
            return (double)epochMillis / 1000;
        }

    }

}