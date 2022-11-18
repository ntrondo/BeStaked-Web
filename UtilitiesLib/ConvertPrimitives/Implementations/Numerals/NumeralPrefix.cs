using System.Security.Cryptography.X509Certificates;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Numeral
{

    /// <summary>
    /// Based on https://en.wikipedia.org/wiki/Metric_prefix
    /// </summary>
    public class NumeralPrefix:Models.Interfaces.INamedScale
    {
        private static readonly string explanationFormat = "'{0}' => '{1}' => {2} {3}"; 
        public static readonly int Base = 10;
        private static readonly Dictionary<int, NumeralPrefix> _metrics = new();
  
        public string Name { get; }
        public string Symbol { get; }
        public int Exponent { get; }

        private double _factor;
        public double Factor 
        {
            get
            {
                if(_factor==0)
                    _factor = Math.Pow(Base, -Exponent);
                return _factor;
            }
        }
        private string? _explanation;
        public string Explanation 
        {
            get
            {
                if (_explanation == null)
                {
                    if (Exponent == 0)
                        _explanation = string.Empty;
                    else
                    {
                        _explanation = string.Format(
                            explanationFormat, 
                            Symbol,
                            Name, 
                            (Exponent > 0 ? "*" : "/"), 
                            Math.Pow(Base, Math.Abs(Exponent)).ToString("N0"));
                    }
                }
                return _explanation;
            }
        }

        internal NumeralPrefix(string name, string symbol, int exponent)
        {
            Name = name;
            Symbol = symbol;
            Exponent = exponent;
        }
    }
}
