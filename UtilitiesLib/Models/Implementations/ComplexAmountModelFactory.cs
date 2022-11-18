using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public class ComplexAmountModelFactory
    {
        public IConvert<double, string> Formatter { get; }
        public ICurrency Currency { get; }
        public IConvert<double, IScaledAmount> Scaler { get; }
        public IConvert<IScaledAmount, string> ScaledFormatter { get; }
        public IConvert<double, ICurrencyAmount> Converter { get; }

        public IComplexConvertedAmountModel Create(double amount)
        {
            IFormattedCurrencyAmount original = new FormattedCurrencyAmountModel(amount, Formatter, Currency);
            return new ComplexAmountModel(amount, Formatter, Currency, Scaler, ScaledFormatter, Converter);
        }
        public ComplexAmountModelFactory(
            IConvert<double,string> formatter, 
            ICurrency currency, 
            IConvert<double, IScaledAmount> scaler,
            IConvert<IScaledAmount, string> scaledFormatter,
            IConvert<double,ICurrencyAmount> converter
            )
        {
            this.Formatter = formatter;
            this.Currency = currency;
            this.Scaler = scaler;
            this.ScaledFormatter = scaledFormatter;
            this.Converter = converter;
        }
    }
}
