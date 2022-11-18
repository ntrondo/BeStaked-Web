using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace Willoch.DemoApp.Client.Code.Convert
{
    public class SharePriceToShortDisplayStringConverter : IConvert<double, string>
    {
        //Should implement converter more like DoubleAmountToShortDisplayStringConverter
        //Multiply with 1e9 and hard coding units is cheating.
        public string Convert(double amount)
        {
            var conv1 = ConstrainedDoubleAmountToShortStringConverter.Instance;
            return conv1.Convert(amount * 1e9) + " HEX / B-Share";
        }

        public string Convert(object value) => Convert(value.ToString());
    }
}
