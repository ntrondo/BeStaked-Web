using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleRounder : ITransform<double>
    {
        private double Factor { get; }
        private RoundingMode RoundingMode { get; }

        public double Convert(double amount)
        {
            bool isNegative = amount < 0;
            double toRound = Math.Abs(amount * Factor);
            double rounded;
            switch (RoundingMode)
            {
                case RoundingMode.TowardsZero: rounded = Math.Floor(toRound);break;
                case RoundingMode.ToNearest: rounded = Math.Round(toRound);break;
                default:rounded = toRound;break;
            }
            double absResult = rounded / Factor;
            double signedResult = (isNegative ? -1 : 1) * absResult;
            return signedResult;
        }
        public DoubleRounder(int decimals, RoundingMode roundingMode = RoundingMode.ToNearest)
        {
            this.RoundingMode = roundingMode;
            this.Factor = Math.Pow(10, decimals);
        }
    }
    public enum RoundingMode
    {
        TowardsZero, ToNearest
    }
}
