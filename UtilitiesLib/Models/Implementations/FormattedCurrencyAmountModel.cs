using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    
    public class FormattedCurrencyAmountModel : CurrencyAmountModel, IFormattedCurrencyAmount
    {
        
        public string AmountString => formatter.Convert(Amount);
        

        private readonly IConvert<double, string> formatter;

        public FormattedCurrencyAmountModel(double amount, IConvert<double,string> formatter, ICurrency currency)
            :base(amount, currency)
        {
            this.formatter = formatter;
            
        }
    }
}
