using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Implementations.Scaled;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Converters;

namespace UtilitiesLibBeStaked.Factories
{
    public class AmountModelFactory
    {
        public static IComplexConvertedAmountModel Create(double amount)
        {
            return Instance.Create(amount);
        }
        public static void SetToken(ICurrency token)
        {
            Token = token;
            instance = null;
        }
        public static void SetConverter(IConvert<double, ICurrencyAmount> converter)
        {
            CurrentConverter = converter;
            instance = null;
        }
        private static IConvert<double, ICurrencyAmount> CurrentConverter = new DoubleToCurrencyConverter(new Currency("Default Fiat", String.Empty, String.Empty), 0);
        private static readonly IConvert<double, string> DefaultFormatter = new DoubleFormatterDependant("###,###,###,###,###,###.########");
        private static ICurrency Token = new Currency("HEX", "HEX");
        private static ComplexAmountModelFactory? instance = null;
        private static ComplexAmountModelFactory Instance
        {
            get
            {
                if (instance == null)
                {                    
                    IConvert<double, IScaledAmount> scaler = new AmountScaler();
                    IConvert<IScaledAmount, string> scaledFormatter = new ScaledAmountToStringConverter(DefaultFormatter);
                    instance = new ComplexAmountModelFactory(DefaultFormatter, Token, scaler, scaledFormatter, CurrentConverter);
                }
                    
                return instance;
            }
        }
    }
}
