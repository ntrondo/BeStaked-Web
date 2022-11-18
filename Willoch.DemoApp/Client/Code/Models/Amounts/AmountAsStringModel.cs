
using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;

namespace Willoch.DemoApp.Client.Code.Models.Amounts
{
    public class AmountAsStringModel : IExposeAmountAsString
    {
        public double Amount { get; }
        protected readonly IConvert<double, string> _stringConverter;
        public string AmountString => this._stringConverter.Convert(this.Amount);
        public AmountAsStringModel(double amount, IConvert<double, string> stringConverter)
        {
            this.Amount = amount;
            this._stringConverter = stringConverter;
        }
        public AmountAsStringModel(double amount, AmountAsStringModel other)
            :this(amount, other._stringConverter)
        {   }
    }
}
