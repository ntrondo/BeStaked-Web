using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public interface IComplexAmountModel : IExposeAmount<double>
    {
        IFormattedCurrencyAmount Original { get; }
        IScaledFormattedCurrencyAmountAsString Scaled { get; }
    }
}
