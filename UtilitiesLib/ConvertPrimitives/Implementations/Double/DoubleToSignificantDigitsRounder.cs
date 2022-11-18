using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleToSignificantDigitsRounder : ITransform<double>
    {
        public int Digits { get; }
        public RoundingMode RoundingMode { get; }

        public DoubleToSignificantDigitsRounder(int digits, RoundingMode roundingMode = RoundingMode.ToNearest)
        {
            this.Digits = digits;
            this.RoundingMode = roundingMode;
        }

        

        public double Convert(double value)
        {
            double absVal = Math.Abs(value);
            //Figure out how many decimals.
            int decimals;
            for (decimals = 0; decimals < Digits; decimals++)
            {
                if (absVal >= Math.Pow(10, Digits - decimals - 1))
                    break;
            }
            var rounder = new DoubleRounder(decimals, RoundingMode);
            return rounder.Convert(value);
        }
    }
}
