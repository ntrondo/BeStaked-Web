namespace UtilitiesLib.ConvertPrimitives.Implementations.Numeral
{
    public class Utilities
    {
        public static NumeralPrefix GetPrefix(double number, NumeralType type, int digits)
        {
            var numeral = Numeral.GetNumeral(type);
            /* Number of scaling operations performed */
            int index = 0;
            if(number==0)
                return numeral.GetPrefix(index);
            /* Scaling factor = 1 000 */
            double factor = Math.Pow(numeral.Base, numeral.ExponentialIncrement);
            double tempNumber = Math.Abs(number);

            /* Target interval */
            int max = (int)Math.Pow(NumeralPrefix.Base, digits);
            double min = Math.Pow(NumeralPrefix.Base, digits - numeral.ExponentialIncrement);
            while (tempNumber >= max)
            {
                tempNumber /= factor;
                index++;
            }
            
            while(tempNumber < min && tempNumber != 0)
            {
                tempNumber *= factor;
                index--;
            }

            
            var mp = numeral.GetPrefix(index);
            return mp;
        }
    }
}
