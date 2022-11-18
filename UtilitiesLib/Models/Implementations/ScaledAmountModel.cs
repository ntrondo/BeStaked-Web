using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    internal class ScaledAmountModel :AmountModel, IScaledAmount
    {
        public ScaledAmountModel(double amount, INamedScale scale):base(amount)
        {
            this.Scale = scale;
        }
        public INamedScale Scale {get;}

    }
}