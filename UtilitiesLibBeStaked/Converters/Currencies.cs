using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib.Models.Implementations;

namespace UtilitiesLibBeStaked.Converters
{
    public class Currencies
    {
        private static readonly Dictionary<string, Currency> _tickerToCurrency = new(new KeyValuePair<string, Currency>[]
        {
            new KeyValuePair<string, Currency>("USD", new Currency("United States Dollar", "USD", "$")),
            new KeyValuePair<string, Currency>("AUD", new Currency("Australian Dollar", "AUD", "$")),
            new KeyValuePair<string, Currency>("CAD", new Currency("Cannadian Dollar", "CAD", "$")),
            new KeyValuePair<string, Currency>("NOK", new Currency("Norwegian Krone", "NOK", "kr")),
            new KeyValuePair<string, Currency>("EUR", new Currency("Euro", "EUR", "€")),
            new KeyValuePair<string, Currency>("GBP", new Currency("Great Britain Pound", "GBP", "£"))
        });
        public static Currency? GetCurrency(string ticker)
        {
            if(_tickerToCurrency.ContainsKey(ticker))
                return _tickerToCurrency[ticker];
            return null;
        }

    }
}
