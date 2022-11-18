namespace UtilitiesLib.Models.Implementations
{
    public interface IComplexConvertedAmountModel : IComplexAmountModel
    {
        IComplexAmountModel Converted { get; }
    }
}
