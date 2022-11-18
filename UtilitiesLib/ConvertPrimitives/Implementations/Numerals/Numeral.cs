namespace UtilitiesLib.ConvertPrimitives.Implementations.Numeral
{
    public class Numeral
    {
        private static readonly Dictionary<NumeralType, Numeral> _numerals = new();
        internal static Numeral GetNumeral(NumeralType type)
        {
            if (_numerals.ContainsKey(type))
                return _numerals[type];
            lock (_numerals)
            {
                if (!_numerals.ContainsKey(type))
                    GenerateNumeral(type);
            }            
            return _numerals[type];
        }

        private static void GenerateNumeral(NumeralType type)
        {   
            if (type == NumeralType.Metric)
            {
                Numeral numeral = new Numeral(type, 10, 3);
                // Based on https://en.wikipedia.org/wiki/Metric_prefix
                numeral.AddPrefix(new NumeralPrefix("yotta", "Y", 24));
                numeral.AddPrefix(new NumeralPrefix("zetta", "Z", 21));
                numeral.AddPrefix(new NumeralPrefix("exa", "E", 18));
                numeral.AddPrefix(new NumeralPrefix("peta", "P", 15));
                numeral.AddPrefix(new NumeralPrefix("tera", "T", 12));
                numeral.AddPrefix(new NumeralPrefix("giga", "G", 9));
                numeral.AddPrefix(new NumeralPrefix("mega", "M", 6));
                numeral.AddPrefix(new NumeralPrefix("kilo", "k", 3));
                numeral.AddPrefix(new NumeralPrefix(String.Empty, String.Empty, 0));
                numeral.AddPrefix(new NumeralPrefix("milli", "m", -3));
                numeral.AddPrefix(new NumeralPrefix("micro", "μ", -6));
                numeral.AddPrefix(new NumeralPrefix("nano", "n", -9));
                numeral.AddPrefix(new NumeralPrefix("pico", "p", -12));
                numeral.AddPrefix(new NumeralPrefix("femto", "f", -15));
                numeral.AddPrefix(new NumeralPrefix("atto", "a", -18));
                numeral.AddPrefix(new NumeralPrefix("zepto", "z", -21));
                numeral.AddPrefix(new NumeralPrefix("yocto", "y", -24));
                _numerals.Add(type, numeral);
            }          
            if(type == NumeralType.ShortScale || type == NumeralType.LongScale)
            {                
                
                string postFix = "illion";
                Numeral numeral = new Numeral(type, 10, 3);
                numeral.AddPrefix(new NumeralPrefix(string.Empty, string.Empty, 0));
                // Based on https://en.wikipedia.org/wiki/Names_of_large_numbers
                numeral.AddPrefix(new NumeralPrefix("Million", "M", 6));
                string[] prefi = new string[] { "B", "Tr", "Quadr", "Quint", "Sext", "Sept", "Oct", "Non" };
                int exp = type == NumeralType.ShortScale ? 6 : 9;
                foreach(var prefix in prefi)
                {
                    string n = prefix + postFix;
                    string s = prefix.First().ToString() + (prefix.Length > 1 ? prefix.Last().ToString() : String.Empty);
                    exp += numeral.ExponentialIncrement;
                    numeral.AddPrefix(new NumeralPrefix(n, s, exp));
                }
                _numerals.Add(type, numeral);
            }
        }

        public NumeralType Type { get; }
        public int Base { get; }
        public int ExponentialIncrement { get; }
        public Numeral(NumeralType type, int numberBase, int exponentialIncrement)
        {
            Type = type;
            this.Base = numberBase;
            this.ExponentialIncrement = exponentialIncrement;
        }
        private readonly Dictionary<int, NumeralPrefix> prefiByExponent = new();
        public void AddPrefix(NumeralPrefix prefix) => this.prefiByExponent.Add(prefix.Exponent, prefix);
        public NumeralPrefix GetPrefix(int index)
        {
            int exponent = index * ExponentialIncrement;
            if (prefiByExponent.ContainsKey(exponent))
                return prefiByExponent[exponent];
            int extExp = prefiByExponent.Keys.Max();
            if (index > extExp)
                return prefiByExponent[extExp];
            return prefiByExponent[prefiByExponent.Keys.Min()];
        }

    }
}
