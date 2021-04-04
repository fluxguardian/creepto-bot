using System;

namespace StrategyTester.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToEpochTime(this DateTime dateTime)
            => (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;

        public static DateTime ToDateTime(this long ticks)
           => new DateTime(1970, 1, 1).AddMilliseconds(ticks);
    }
}
