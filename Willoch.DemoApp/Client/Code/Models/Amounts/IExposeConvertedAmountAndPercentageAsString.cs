namespace Willoch.DemoApp.Client.Code.Models.Amounts
{
    public interface IExposeConvertedAmountAndPercentageAsString: IExposeConvertedAmountAsString
    {
        IExposeAmountAsString Percentage { get; }
    }
}
