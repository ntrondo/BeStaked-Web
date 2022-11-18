namespace UtilitiesLib.Models.Interfaces
{
    public interface ICurrency:IExposeName,IExposeSymbol,IExposeTicker
    {
        string SymbolOrTicker { get; }
    }
}
