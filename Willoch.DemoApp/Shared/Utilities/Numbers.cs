using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Willoch.DemoApp.Shared.Utilities
{
    public static class Numbers
    {
        private static readonly string HexPrefix = "0x";
        private static readonly string HexToUInt64Prefix = HexPrefix + "0000000";
        private static readonly string UHex64MaxValue = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        private static readonly double UInt256MaxValue = (double)BigInteger.Parse("0" + UHex64MaxValue, System.Globalization.NumberStyles.AllowHexSpecifier);
        public static double ConvertToTokenAmount(string longAmountString, ERC20Info contractInfo)
        {
            if (longAmountString.StartsWith(HexToUInt64Prefix))
            {
                //Amount is smaller than some number
                UInt64 longAmount = Convert.ToUInt64(longAmountString, 16);
                return ConvertToTokenAmount(longAmount, contractInfo);
            }  
            else
            {
                //Amount is too large for UInt64
                if (longAmountString.StartsWith(HexPrefix))
                    //Remove prefix. Parser does not like it.
                    longAmountString = longAmountString.Substring(2);
                if (!longAmountString.StartsWith("0"))
                    //Prepend zero. Input is unsigned. Avoid negative outputs.
                    //https://stackoverflow.com/questions/30119174/converting-a-hex-string-to-its-biginteger-equivalent-negates-the-value
                    longAmountString = "0" + longAmountString;
                BigInteger bigAmount = BigInteger.Parse(longAmountString, System.Globalization.NumberStyles.AllowHexSpecifier);
                return ConvertToTokenAmount((double)bigAmount, contractInfo);
            }    
        }
        public static double GetMaximumTokenAmount(ERC20Info contractInfo)
        {
            return ConvertToTokenAmount(UInt256MaxValue, contractInfo);
        }
        public static double ConvertToTokenAmount(UInt64 longAmount, ERC20Info contractInfo)
        {
            var divisor = Math.Pow(10, contractInfo.Decimals);
            double amount = longAmount / divisor;
            return amount;
        }
        public static double ConvertToTokenAmount(double longAmount, ERC20Info contractInfo)
        {
            if (longAmount > UInt256MaxValue)
                longAmount = UInt256MaxValue;
            var divisor = Math.Pow(10, contractInfo.Decimals);
            double amount = longAmount / divisor;
            return amount;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shortAmount">Token amount</param>
        /// <param name="contractInfo"></param>
        /// <returns></returns>
        public static double ConvertFromTokenAmountToDouble(double shortAmount, ERC20Info contractInfo)
        {
            var factor = Math.Pow(10, contractInfo.Decimals);
            var longAmount = shortAmount * factor;
            return Math.Min(longAmount, UInt256MaxValue);
        }
        public static ulong ConvertFromTokenAmount(double shortAmount, ERC20Info contractInfo)
        {
            var factor = Math.Pow(10, contractInfo.Decimals);
            var longAmount = shortAmount * factor;
            if(ulong.MaxValue < longAmount)
                //The ulong is implicitly parsed to double for comparison.
                //Implicit parsing uses the type of the right argument as preferred type in comparisons.
                return ulong.MaxValue;
            return (ulong)longAmount;
        }

        public static BigInteger ParseToBigInteger(string longHexIntegerString)
        {
            string prefix = "0x00";
            string newPrefix = prefix.Substring(0, 3);
            while (longHexIntegerString.StartsWith(prefix))
                longHexIntegerString = longHexIntegerString.Replace(prefix, newPrefix);
            longHexIntegerString = longHexIntegerString.Replace(newPrefix, "0");
            return BigInteger.Parse(longHexIntegerString, System.Globalization.NumberStyles.AllowHexSpecifier);
        }
        public static string ToHexaString(double value)
        {
            if( ulong.MaxValue >= value)
                return ToHexaString((ulong)value);
            if (value >= UInt256MaxValue)
                return UHex64MaxValue;

            var bi = (BigInteger)value;
            string signedHexaString = bi.ToString("X");
            
            if (signedHexaString.Length == 65 && signedHexaString.StartsWith("0"))
                //Unsign
                signedHexaString = signedHexaString.Substring(1);
            return signedHexaString;
        }


        public static string ToHexaString(int value)
        {
            return value.ToString("X");
        }
        public static string ToHexaString(ulong value)
        {
            return value.ToString("X");
        }

    }
}
