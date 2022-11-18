using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Converters;
using Willoch.DemoApp.Client.Services;

namespace UnitTests.UI.Services
{
    internal class MockExchangeRatesService : BaseExchangeRateService
    {
        internal bool ReturnFlag { get; set; }
        protected override Task<HEXToFiatCurrencyConverter> CreateLoadingTaskVirtual(string fromTicker, string toTicker)
        {
            var c = Currencies.GetCurrency(toTicker);
            if (c == null)
                throw new NotSupportedException();
            return ReturnWhenFlagged(new HEXToFiatCurrencyConverter(c, 0.04));
        }
        private async Task<HEXToFiatCurrencyConverter> ReturnWhenFlagged(HEXToFiatCurrencyConverter converter)
        {
            while (!ReturnFlag)
                await Task.Delay(100);
            return converter;
        }
    }
}
