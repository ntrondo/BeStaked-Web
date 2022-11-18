using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleToCurrencyConverter : IConvert<double, ICurrencyAmount>
    {
        public ICurrency Currency { get; }
        public double Factor { get; }

        public ICurrencyAmount Convert(double value)
        {
            return new CurrencyAmountModel(value * Factor, Currency);
        }
        public DoubleToCurrencyConverter(ICurrency currency, double factor)
        {
            this.Currency = currency;
            this.Factor = factor;
        }
    }
    public class DoubleToCachedCurrencyConverter : BaseClasses.CachedConverterBase<double, ICurrencyAmount>
    {
        private IConvert<double,ICurrencyAmount> InnerConverter { get; } 
        public override ICurrencyAmount convert(double value)
        {
            return InnerConverter.Convert(value);
        }
        private ICurrencyAmount? _cachedDefault = null;
        protected override ICurrencyAmount? GetDefaultValue()
        {
            if (_cachedDefault == null)
                _cachedDefault = convert(default);
            return _cachedDefault;
        }
        public DoubleToCachedCurrencyConverter(ICurrency currency, double factor)
        {
            InnerConverter = new DoubleToCurrencyConverter(currency, factor);
        }
    }
}
