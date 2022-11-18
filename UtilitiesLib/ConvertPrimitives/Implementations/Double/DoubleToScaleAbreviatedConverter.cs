using UtilitiesLib.ConvertPrimitives.Implementations.Numeral;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleToShortScaleAbreviationConverter : BaseClasses.CachedConverterBase<double, IScaledAmount>
    {
        private readonly ITransform<double> rounder;
        public NumeralType NumeralType { get; }
        public int Digits { get; }

        public DoubleToShortScaleAbreviationConverter(NumeralType numeralType, int digits)
        {
            this.NumeralType = numeralType;
            this.Digits = digits;
            this.rounder = new DoubleToSignificantDigitsRounder(Digits);
        }
        public override IScaledAmount convert(double value)
        {
            INamedScale scale = Utilities.GetPrefix(value, NumeralType, Digits);
            double scaledValue = value * scale.Factor;
            var roundedValue = rounder.Convert(scaledValue);
            return new Models.Implementations.ScaledAmountModel(roundedValue, scale);
        }
        private IScaledAmount? _cachedDefault;
        protected override IScaledAmount? GetDefaultValue()
        {
            if(_cachedDefault == null)
                _cachedDefault = convert(0);
            return _cachedDefault;
        }
    }
}
