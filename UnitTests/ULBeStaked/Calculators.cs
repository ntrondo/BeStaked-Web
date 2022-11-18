using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLibBeStaked.Converters;
using UtilitiesLibBeStaked.Factories;
using Xunit;

namespace UnitTests.ULBeStaked
{
    public class Calculators
    {
        [Fact]
        public void SharesCalculatorTest()
        {
            double currentSharePrice = 250027e-13;
            SharesCalculator converter = new SharesCalculator(currentSharePrice);

            //10 000 HEX for 1000 days
            var result = converter.Convert(new(1e4, 1000));
            Assert.NotNull(result);
            var amount = AmountModelFactory.Create(result == null ? 0 : result.Shares);
            Assert.Equal(619, amount.Scaled.Amount);
            Assert.Equal(1e-9, amount.Scaled.Scale.Factor);

            result = converter.Convert(new(449e3, 5000));
            amount = AmountModelFactory.Create(result == null ? 0 : result.Shares);
            Assert.Equal(53.9, amount.Scaled.Amount);
            Assert.Equal(1e-12, amount.Scaled.Scale.Factor);
        }

        [Fact]
        public void PrincipalCalculator()
        {
            PrincipalCalculator converter = new PrincipalCalculator(0.1);
            double currentSharePrice = 250027e-13;

            var result = converter.Convert(new((ulong)619e9, 1000, currentSharePrice));
            var amount = AmountModelFactory.Create(result);
            Assert.Equal(1e-3, amount.Scaled.Scale.Factor);
            Assert.Equal(9.99, amount.Scaled.Amount);

            result = converter.Convert(new((ulong)53.9e12, 5000, currentSharePrice));
            amount = AmountModelFactory.Create(result);
            Assert.Equal(1e-3, amount.Scaled.Scale.Factor);
            Assert.Equal(449, amount.Scaled.Amount);
        }
    }
}
