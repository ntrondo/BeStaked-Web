using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;
namespace Willoch.DemoApp.Client.Code.Models.Amounts
{
    //public class AmountConvertedAndAsStringModel : AmountAsStringModel, IExposeConvertedAmountAsString
    //{
    //    protected readonly ITransform<double> _amountConverter;
    //    public AmountConvertedAndAsStringModel(
    //        double amount,
    //        IConvert<double, string> stringConverter,
    //        ITransform<double> amountConverter) 
    //        : base(amount, stringConverter)
    //    {
    //        this._amountConverter = amountConverter;
    //    }
    //    private IExposeAmountAsString _converted;
    //    public IExposeAmountAsString Converted 
    //    {
    //        get
    //        {
    //            if (this._converted == null)
    //            {
    //                if (this._amountConverter == null)
    //                    this._converted = new AmountAsStringModel(default, this._stringConverter);
    //                else
    //                    this._converted = new AmountAsStringModel(this._amountConverter.Convert(this.Amount), this._stringConverter);
    //            }
    //            return this._converted;
    //        }
    //    }
    //}
}
