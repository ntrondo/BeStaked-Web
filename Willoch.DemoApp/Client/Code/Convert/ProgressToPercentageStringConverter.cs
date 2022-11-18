using System;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace Willoch.DemoApp.Client.Code.Convert
{
    public class PercentageToDisplayStringConverter : IConvert<double, string>
    {
        private static readonly string format = "{0} %";
        public PercentageToDisplayStringConverter(int decimals = 0, RoundingMode roundingMode = RoundingMode.TowardsZero)
        {            
            this.Rounder = new DoubleRounder(decimals, roundingMode);
        }
        
        private DoubleRounder Rounder { get; }
        public virtual string Convert(object value)
        {
            if (value is double dbl)
                return this.Convert(dbl);
            return String.Empty;
        }

        public string Convert(double amount)
        {
            double rounded = Rounder.Convert(amount);
            return string.Format(format, rounded);
        }
    }    
   
    public abstract class BaseConvertedPercentageToDisplayStringConverter : PercentageToDisplayStringConverter
    {
        public IConvert<object, double> ValueToDoubleConverter { get; }
        protected BaseConvertedPercentageToDisplayStringConverter(IConvert<object, double> valueToDoubleConverter, int decimals = 0, RoundingMode roundingMode = RoundingMode.TowardsZero) 
            : base(decimals, roundingMode)
        {
            this.ValueToDoubleConverter = valueToDoubleConverter;
        }
        public override string Convert(object value)
        {
            double progress = ValueToDoubleConverter.Convert(value);
            return base.Convert(progress);
        }
    }
    public class DateProgressToPercentageStringConverter : BaseConvertedPercentageToDisplayStringConverter, IConvert<DateTime, string>
    {
        public DateProgressToPercentageStringConverter(DateTime start, DateTime end,int decimals = 0, RoundingMode roundingMode = RoundingMode.TowardsZero)
            :this(new DateProgressPercentageConverter(start, end), decimals, roundingMode) { }
        public DateProgressToPercentageStringConverter(IConvert<object, double> valueToDoubleConverter, int decimals = 0, RoundingMode roundingMode = RoundingMode.TowardsZero)
            :base(valueToDoubleConverter, decimals, roundingMode) { }
        public string Convert(DateTime date) => Convert((object)date);     
    }
   
}
