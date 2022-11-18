using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public class ScaledCurrencyAmountModel : CurrencyAmountModel, IScaledFormattedCurrencyAmountAsString
    {
        public INamedScale Scale => Scaled.Scale;

        public string AmountString => Formatter.Convert(Scaled);

        private IScaledAmount Scaled { get; }
        private IConvert<IScaledAmount, string> Formatter { get; }

        //public ScaledCurrencyAmountModel(double amount, INamedScale scale, IConvert<double, string> formatter, ICurrency currency) 
        //    : base(amount, currency)
        //{
        //    this.Scale = scale;
        //}

        public ScaledCurrencyAmountModel(IScaledAmount scaled, IConvert<IScaledAmount, string> scaledFormatter, ICurrency currency)
            :base(scaled.Amount, currency)
        {
            this.Scaled = scaled;
            this.Formatter = scaledFormatter;
        }
    }
}
