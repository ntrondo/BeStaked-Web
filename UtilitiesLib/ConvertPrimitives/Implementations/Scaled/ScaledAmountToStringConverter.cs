using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Scaled
{
    public class ScaledAmountToStringConverter : IConvert<IScaledAmount, string>
    {
        private static ScaledAmountToStringConverter? instance = null;
        public static IConvert<IScaledAmount, string> Instance
        {
            get
            {
                if(instance == null)
                    instance = new ScaledAmountToStringConverter();
                return instance;
            }
        }
        private IConvert<double, string> innerFormatter;

        public string Convert(IScaledAmount value)
        {
            return innerFormatter.Convert(value.Amount) + value.Scale.Symbol;
        }
        private ScaledAmountToStringConverter() 
        {
            innerFormatter = new DoubleFormatterG();
        }

        public ScaledAmountToStringConverter(IConvert<double, string> innerFormatter)
        {
            this.innerFormatter = innerFormatter;
        }
    }
}
