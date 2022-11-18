namespace Willoch.DemoApp.Client.Code.Models.Amounts
{
    public interface IExposeAmountAsString:IExposeAmount
    {        
        public string AmountString { get; }
    }
}
