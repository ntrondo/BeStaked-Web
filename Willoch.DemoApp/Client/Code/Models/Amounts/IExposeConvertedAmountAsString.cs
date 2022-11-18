namespace Willoch.DemoApp.Client.Code.Models.Amounts
{
    public interface IExposeConvertedAmountAsString:IExposeAmountAsString
    {
        IExposeAmountAsString Converted { get; }
    }
}
