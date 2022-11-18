using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public class DoubleParser : IConvert<string,double>
    {
        private static readonly char space = ' ';
        public double Convert(string text)
        {
            double result = 0;
            if(double.TryParse(text, out result))
                return result;
            if (text.Contains(space))
                return Convert(string.Join(string.Empty, text.Split(space)));            
            return default;
        }

        public double Convert(object input)
        {
            if(input is double dbl)
                return dbl;
            string? text = input as string;
            if(text != null)
                return Convert(text);
            return default;
        }
    }
}
