using System;

namespace Willoch.DemoApp.Client.Code.Calculate
{
    public class PercentageCalculator
    {
        public double GetResult(double value, double maximum)
        {
            return 100 * (value / maximum);
        }
    }
}
