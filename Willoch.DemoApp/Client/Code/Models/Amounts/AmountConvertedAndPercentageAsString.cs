using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;

namespace Willoch.DemoApp.Client.Code.Models.Amounts
{
    //public class AmountConvertedAndPercentageAsStringModel : AmountConvertedAndAsStringModel, IExposeConvertedAmountAndPercentageAsString
    //{
    //    public AmountConvertedAndPercentageAsStringModel(double amount, double baseAmount, IConvert<double, string> stringConverter, ITransform<double> amountConverter) 
    //        : base(amount, stringConverter, amountConverter)
    //    {
    //        this.BaseAmount = baseAmount;
    //    }
    //    private IExposeAmountAsString _percentage;
    //    public IExposeAmountAsString Percentage
    //    {
    //        get
    //        {
    //            if(this._percentage == null)
    //            {
    //                double percentage = new Calculate.PercentageCalculator().GetResult(this.Amount, this.BaseAmount);
    //                this._percentage = new AmountAsStringModel(percentage, new PercentageToDisplayStringConverter());
    //            }
    //            return this._percentage;
    //        }
    //    }

    //    public double BaseAmount { get; }
    //}
}
