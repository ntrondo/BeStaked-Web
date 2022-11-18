using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public class BaseComplexAmountModel:IComplexAmountModel
    {
        public double Amount => Original.Amount;
        protected readonly IConvert<double, string> formatter;
        public IFormattedCurrencyAmount Original { get; }

        protected readonly IConvert<double, IScaledAmount> scaler;

        protected readonly IConvert<IScaledAmount, string> scaledFormatter;
        private ScaledCurrencyAmountModel? _scaled = null;
        public IScaledFormattedCurrencyAmountAsString Scaled
        {
            get
            {
                if (_scaled == null)
                {
                    var scaled = scaler.Convert(this.Amount);
                    _scaled = new ScaledCurrencyAmountModel(scaled, scaledFormatter, Original.Currency);
                }
                return _scaled;
            }
        }
        //public BaseComplexAmountModel(
        //    IFormattedCurrencyAmount original,
        //    IConvert<double, IScaledAmount> scaler,
        //    IConvert<double, string> formatter)
        //{
        //    this.Original = original;
        //    this.scaler = scaler;
        //    this.formatter = formatter;
        //}

        public BaseComplexAmountModel(
            double amount, 
            IConvert<double, string> formatter, 
            ICurrency currency, 
            IConvert<double, IScaledAmount> scaler,
            IConvert<IScaledAmount, string> scaledFormatter
            )
        {
            this.Original = new FormattedCurrencyAmountModel(amount, formatter, currency);
            this.scaler = scaler;
            this.formatter = formatter;
            this.scaledFormatter = scaledFormatter;
        }
    }
}
