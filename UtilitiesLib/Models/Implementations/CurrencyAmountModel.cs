using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public class Currency : ICurrency
    {     
        public string Name { get; }
        public string Ticker { get; }
        public string? Symbol { get; }
        public string SymbolOrTicker => string.IsNullOrEmpty(Symbol) ? Ticker : Symbol;
        public Currency(string name, string ticker, string? symbol = null)
        {
            Name = name;
            Ticker = ticker;
            Symbol = symbol;            
        }
    }
    public class CurrencyAmountModel : AmountModel, ICurrencyAmount
    {
        public ICurrency Currency { get; }
        public CurrencyAmountModel(double amount, ICurrency currency) : base(amount)
        {
            this.Currency = currency;
        }
    }
}
