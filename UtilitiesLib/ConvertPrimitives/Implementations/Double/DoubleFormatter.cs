using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace UtilitiesLib.ConvertPrimitives.Implementations.Double
{
    public abstract class DoubleFormatter : IConvert<double, string>
    {
        public abstract string Convert(double value);
    }
    public class DoubleFormatterDependant : DoubleFormatter
    {
        public string FormatSpecifier { get; }
        public override string Convert(double value)
        {
            return value.ToString(FormatSpecifier);
        }
        public DoubleFormatterDependant(string formatSpecifier)
        {
            FormatSpecifier = formatSpecifier;
        }
    }
  
    internal class DoubleFormatterIndependant : DoubleFormatter
    {
        public NumberFormatInfo NumberFormatInfo { get; }
        

        public override string Convert(double value)
        {
            return value.ToString(NumberFormatInfo);
        }

        internal DoubleFormatterIndependant(NumberFormatInfo numberFormat)
        {
            this.NumberFormatInfo = numberFormat;
        }
    }
    public class DoubleFormatterG : DoubleFormatterDependant
    {
        public DoubleFormatterG() : base("G")
        {
        }
    }
    public class DoubleFormatterN : DoubleFormatterDependant
    {
        public DoubleFormatterN(int decimals = 0) : base("N" + decimals)
        {
        }
    }
}
