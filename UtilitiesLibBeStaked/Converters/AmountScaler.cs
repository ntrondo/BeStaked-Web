
using UtilitiesLib.ConvertPrimitives.BaseClasses;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Implementations.Numeral;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLibBeStaked.Converters
{
    public class AmountScaler : CachedConverterBase<double, IScaledAmount>
    {
        private readonly IConvert<double, IScaledAmount> MetricScaler = new DoubleToShortScaleAbreviationConverter(NumeralType.Metric, 3);
        private readonly IConvert<double, IScaledAmount> ShortScaler = new DoubleToShortScaleAbreviationConverter(NumeralType.ShortScale, 3);
        private static readonly double[] MetricInterval = new double[] { 999.5, 999500 };
        public override IScaledAmount convert(double value)
        {
            double absVal = Math.Abs(value);
            if (MetricInterval[0] <= absVal && absVal < MetricInterval[1])
                return MetricScaler.Convert(value);
            return ShortScaler.Convert(value);            
        }
        protected override IScaledAmount? GetDefaultValue()
        {
            return MetricScaler.Convert(default);
        }
    }
}
