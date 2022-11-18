using System;

namespace Willoch.DemoApp.Client.Code.Calculate
{
    public class TimeSpanToDaysCalculator
    {
        public double GetResult(TimeSpan time)
        {
            return time.TotalDays;
        }
        public double GetResult(DateTime start, DateTime end) => GetResult(end - start);
    }
}
