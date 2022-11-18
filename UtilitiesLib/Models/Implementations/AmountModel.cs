using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.Models.Implementations
{
    public class AmountModel : IExposeAmount<double>
    {
        public double Amount { get; }
        public AmountModel(double amount)
        {
            Amount = amount;
        }
    }
}
