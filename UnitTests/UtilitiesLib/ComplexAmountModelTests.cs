using System;
using UtilitiesLib.ConvertPrimitives.Implementations.Scaled;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Converters;
using Xunit;
using UtilitiesLib.Models.Implementations;
using UtilitiesLibBeStaked.Factories;

namespace UnitTests.UtilitiesLib
{
    public class ComplexAmountModelTests
    {
        [Fact]
        public void ScaledMetricNumeral()
        {
            IScaledAmount amount;
            IConvert<double,IScaledAmount> scaler = new DoubleTo3DigitsMetricAbreviationConverter();

            amount = scaler.Convert(-123456e10);
            Assert.Equal(-1.23, amount.Amount);
            Assert.Equal("peta", amount.Scale.Name);

            amount = scaler.Convert(-1.023e-10);
            Assert.Equal(-102, amount.Amount);
            Assert.Equal("pico", amount.Scale.Name);

            amount = scaler.Convert(0);
            Assert.Equal(0,amount.Amount);
            Assert.Equal(string.Empty, amount.Scale.Name);

            amount = scaler.Convert(999999e-16);
            Assert.Equal(100, amount.Amount);
            Assert.Equal("pico", amount.Scale.Name);

            amount = scaler.Convert(200e3);
            Assert.Equal(200, amount.Amount);
            Assert.Equal("kilo", amount.Scale.Name);

            amount = scaler.Convert(555555e20);
            Assert.Equal(55.6, amount.Amount);
            Assert.Equal("yotta", amount.Scale.Name);
        }
        [Fact]
        public void ScaledHybridNumeral()
        {
            IScaledAmount amount;
            IConvert<double, IScaledAmount> scaler = new AmountScaler();

            //Large negative amount is labeled according to short scale
            amount = scaler.Convert(-123456e10);
            Assert.Equal(-1.23, amount.Amount);
            Assert.Equal("Quadrillion", amount.Scale.Name);

            //Tiny amounts are recognized as zero
            amount = scaler.Convert(-1.023e-10);
            Assert.Equal(0, amount.Amount);
            Assert.Equal(String.Empty, amount.Scale.Name);

            //Zero input does not break scaling logic
            amount = scaler.Convert(0);
            Assert.Equal(0, amount.Amount);
            Assert.Equal(string.Empty, amount.Scale.Name);

            //Thousands are scaled using the metric 'kilo' notation
            amount = scaler.Convert(200e3);
            Assert.Equal(200, amount.Amount);
            Assert.Equal("kilo", amount.Scale.Name);

            //Large amount is labeled according to short scale
            amount = scaler.Convert(555555e20);
            Assert.Equal(55.6, amount.Amount);
            Assert.Equal("Septillion", amount.Scale.Name);            
        }
        [Fact]        
        public void ScaleCaching()
        {
            //Converting the same amount again yields a reference to same cached object.
            IConvert<double, IScaledAmount> scaler = new AmountScaler();
            double amount = 23.4e6;
            IScaledAmount amount1 = scaler.Convert(amount);
            IScaledAmount amount2 = scaler.Convert(amount);
            Assert.True(amount1 == amount2);

            //Also for zero
            amount = 0;
            amount1 = scaler.Convert(amount);
            amount2 = scaler.Convert(amount);
            Assert.True(amount1 == amount2);
        }
        [Fact]
        public void ScaledAmountFormatting()
        {
            IConvert<double, IScaledAmount> scaler = new AmountScaler();
            IConvert<IScaledAmount, string> formatter = ScaledAmountToStringConverter.Instance;
            double amount;
            string text;

            amount = -1.435e8;
            text = formatter.Convert(scaler.Convert(amount));
            Assert.Equal("-144M", text);

            amount = 0;
            text = formatter.Convert(scaler.Convert(amount));
            Assert.Equal("0", text);

            amount = 4000;
            text = formatter.Convert(scaler.Convert(amount));
            Assert.Equal("4k", text);

            amount = 1.435e9;
            text = formatter.Convert(scaler.Convert(amount));
            Assert.Equal(1.44 + "B", text);

            amount = 1.435e18;
            text = formatter.Convert(scaler.Convert(amount));
            Assert.Equal(1.44 + "Qt", text);
        }
        [Fact]
        public void AmountConversion()
        {
            ICurrency usd = new Currency("United States Dollar", "USD", "$");
            ICurrency hex = new Currency("HEX", "HEX");
            double factor = 0.04;
            IConvert<double, ICurrencyAmount> hexToUsdConverter = new DoubleToCachedCurrencyConverter(usd, factor);
            IConvert<double, ICurrencyAmount> usdToHexConverter = new DoubleToCachedCurrencyConverter(hex, 1.0/factor);
            ICurrencyAmount usdAmount, hexAmount;

            //Simple conversion
            usdAmount = new CurrencyAmountModel(100, usd);
            hexAmount = usdToHexConverter.Convert(usdAmount.Amount);
            Assert.Equal(2500, hexAmount.Amount);

            //Convert back
            usdAmount = hexToUsdConverter.Convert(hexAmount.Amount);
            Assert.Equal(100, usdAmount.Amount);

            //Cached
            var anotherHexAmount = usdToHexConverter.Convert(usdAmount.Amount);
            Assert.True(hexAmount == anotherHexAmount);
        }
        [Fact]
        public void AmountModel()
        {
            IConvert<double, string> amountFormatter = new DoubleFormatterN();
            IConvert<IScaledAmount, string> scaledFormatter = ScaledAmountToStringConverter.Instance;
            ICurrency currency = new Currency("HEX", "HEX");
            IConvert<double, IScaledAmount> scaler = new AmountScaler();
            IConvert<double, ICurrencyAmount> converter = new DoubleToCachedCurrencyConverter(new Currency("United States Dollar", "USD", "$"), 0.04);
            
            ComplexAmountModelFactory factory = new ComplexAmountModelFactory(amountFormatter, currency, scaler, scaledFormatter, converter);

            IComplexConvertedAmountModel amount = factory.Create(1e5);

            Assert.Equal(currency.SymbolOrTicker, amount.Original.Currency.SymbolOrTicker);
            Assert.Equal("100 000", amount.Original.AmountString);
            Assert.Equal(100, amount.Scaled.Amount);
            Assert.Equal("kilo", amount.Scaled.Scale.Name);
            Assert.Equal("100k", amount.Scaled.AmountString);
            Assert.Equal("4k", amount.Converted.Scaled.AmountString);

            amount = factory.Create(41333);

            Assert.Equal("41 333", amount.Original.AmountString);
            Assert.Equal(41.3, amount.Scaled.Amount);
            Assert.Equal("kilo", amount.Scaled.Scale.Name);
            Assert.Equal((41.3).ToString() + "k", amount.Scaled.AmountString);

            amount = factory.Create(2.33333);
            Assert.Equal("2", amount.Original.AmountString);
        }
        [Fact]
        public void ComplexAmountModelFactory()
        {
            IComplexConvertedAmountModel amount = AmountModelFactory.Create(1e5);

            Assert.Equal("100 000", amount.Original.AmountString);
            Assert.Equal(100, amount.Scaled.Amount);
            Assert.Equal("kilo", amount.Scaled.Scale.Name);
            Assert.Equal("100k", amount.Scaled.AmountString);

            amount = AmountModelFactory.Create(41333);

            Assert.Equal("41 333", amount.Original.AmountString);
            Assert.Equal(41.3, amount.Scaled.Amount);
            Assert.Equal("kilo", amount.Scaled.Scale.Name);
            Assert.Equal((41.3).ToString() + "k", amount.Scaled.AmountString);

            amount = AmountModelFactory.Create(2.33333);
            Assert.Equal(amount.Original.Amount.ToString(), amount.Original.AmountString);
        }
    }
}
