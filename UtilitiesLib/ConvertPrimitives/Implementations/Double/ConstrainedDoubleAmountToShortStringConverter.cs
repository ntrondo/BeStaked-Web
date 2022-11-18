namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public sealed class ConstrainedDoubleAmountToShortStringConverter:DoubleAmountToShortDisplayStringConverter
    {
        private static ConstrainedDoubleAmountToShortStringConverter? instance = null;
        public static Interfaces.IConvert<double,string> Instance
        { 
            get 
            {
                if (instance == null)
                    instance = new ConstrainedDoubleAmountToShortStringConverter();
                return instance; 
            } 
        }
        private static readonly double InfinityLimit = 999 * Math.Pow(10, 3 * (ScaledNumberUnits.Length - 1));//999e21;
        

        public override string convert(double amount)
        {
            if (amount == 0)
                return "-";
            if (amount > InfinityLimit)
                return "infinite";
            return base.convert(amount);
        }
    }
}
