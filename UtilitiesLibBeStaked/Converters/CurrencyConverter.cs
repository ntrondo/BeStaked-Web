using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;

namespace UtilitiesLibBeStaked.Converters
{
    public class HEXToFiatCurrencyConverter : DoubleToCachedCurrencyConverter,ITransform<double>
    {
        public HEXToFiatCurrencyConverter(ICurrency fiat, double factor) : base(fiat, factor)
        {
        }
        /// <summary>
        /// Transitional implementation. Remove!
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        double IConvert<double, double>.Convert(double value)
        {
            var amount = base.Convert(value);
            if (amount == null)
                return 0;
            return amount.Amount;
        }
        public class Serializable
        {
            [System.Text.Json.Serialization.JsonPropertyName("f")]
            public double Factor { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("t")]
            public string Ticker { get; set; } = string.Empty;

            public Serializable(HEXToFiatCurrencyConverter converter)
            {
                var unityConversion = converter.Convert(1);
                if(unityConversion != null)
                {
                    this.Factor = unityConversion.Amount;
                    this.Ticker = unityConversion.Currency.Ticker;
                }               
            }
            public Serializable() { }
        }
    }
    
}
