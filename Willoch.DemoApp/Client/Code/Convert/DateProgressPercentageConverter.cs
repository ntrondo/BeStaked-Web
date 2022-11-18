using System;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace Willoch.DemoApp.Client.Code.Convert
{
    public class DateProgressPercentageConverter : IConvert<DateTime, double>, IConvert<object, double>
    {
        public DateProgressPercentageConverter(DateTime start, DateTime end)
        {
            this.Start = start;
            this.TotalDays = (end - start).TotalDays;

        }
        private DateTime Start { get; }
        private double TotalDays { get; }

        public double Convert(object input)
        {
            if (input is DateTime dt)
                return Convert(dt);
            return 0;
        }
        private Calculate.TimeSpanToDaysCalculator DaysCalculator = new Calculate.TimeSpanToDaysCalculator();
        private Calculate.PercentageCalculator PercentageCalculator = new Calculate.PercentageCalculator();
        public double Convert(DateTime value)
        {
            var lapsedDays = DaysCalculator.GetResult(value - Start);
            return PercentageCalculator.GetResult(lapsedDays, TotalDays);
        }
    }
}
