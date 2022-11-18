using UtilitiesLib.ConvertPrimitives.Implementations.Numeral;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleTo3DigitsShortAbreviationConverter : DoubleToShortScaleAbreviationConverter
    {
        public DoubleTo3DigitsShortAbreviationConverter()
      : base(NumeralType.ShortScale, 3) { }
    }
}
