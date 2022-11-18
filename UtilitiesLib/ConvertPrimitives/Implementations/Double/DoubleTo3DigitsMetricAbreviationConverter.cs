using UtilitiesLib.ConvertPrimitives.Implementations.Numeral;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleTo3DigitsMetricAbreviationConverter : DoubleToShortScaleAbreviationConverter
    {
        public DoubleTo3DigitsMetricAbreviationConverter() 
            : base(NumeralType.Metric, 3) {  }
    }
}
