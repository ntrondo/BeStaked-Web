namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    /// <summary> 
    /// Converts numbers to 3 digit representation with .
    /// Appends letters to indicate order of magnitude.
    /// </summary>
    public class DoubleAmountToShortDisplayStringConverter : BaseClasses.CachedConverterBase<double, string>
    {
        protected static readonly string[] ScaledNumberUnits = new string[] { "", "K", "M", "B", "T", "QD", "QT", "SX" };
        public override string convert(double amount)
        {
            int unitIndex = 0;
            while (amount > 999)
            {
                unitIndex++;
                amount /= 1000;
            }
            if (unitIndex >= ScaledNumberUnits.Length)
                return "infinite";
            string unit = ScaledNumberUnits[unitIndex];
            if (amount <= 0.005)
                return "0";
            if (amount < 10)
                return Math.Round(amount, 2).ToString() + unit;
            if (amount < 100)
                return Math.Round(amount, 1).ToString() + unit;
            return Math.Round(amount, 0).ToString() + unit;
        }
    }
}
