using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public class ComplexAmountModel: BaseComplexAmountModel, IComplexConvertedAmountModel
    {
    
        private readonly IConvert<double, ICurrencyAmount> converter;
        private BaseComplexAmountModel? _converted = null;

        public IComplexAmountModel Converted 
        {
            get
            {
                if(_converted == null)
                {
                    var result = converter.Convert(this.Amount);
                    _converted = new BaseComplexAmountModel(result.Amount, formatter, result.Currency, scaler, scaledFormatter);
                }
                return _converted;
            }
        }


        public ComplexAmountModel(
            double amount, 
            IConvert<double, string> formatter, 
            ICurrency currency, 
            IConvert<double, IScaledAmount> scaler, 
            IConvert<IScaledAmount, string> scaledFormatter, 
            IConvert<double, ICurrencyAmount> converter)
            :base(amount, formatter, currency, scaler, scaledFormatter)
        {
            this.converter = converter;
        }
    }
}
