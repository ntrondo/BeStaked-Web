using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesLib.ConvertPrimitives.BaseClasses;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.NumMeth.LinConv;

namespace UtilitiesLibBeStaked.Converters
{
    public class PrincipalCalculator : CachedConverterBase<PrincipalCalculator.InputModel, double>
    {
        public double Accuracy { get; }
        public PrincipalCalculator(double accuracy)
        {
            Accuracy = accuracy;
        }

        public override double convert(InputModel value)
        {
            IConvert<SharesCalculator.InputModel, SharesCalculator.OutputModel> shareCalc = new SharesCalculator(value.SharePrice);
            var getSharesFunc = new Func<double, double>(amount => shareCalc.Convert(new(amount, value.Duration)).Shares);
            double guess = value.SharePrice * value.Shares / 2;
            var converger = new LinearConverger( value.Shares, getSharesFunc, this.Accuracy, guess - 100, guess + 100);
            var principal = converger.Converge() ? converger.Result : 0;
            
            return principal;
        }

        public class InputModel
        {
            public ulong Shares { get; }
            public int Duration { get; }
            public double SharePrice { get; }
            public InputModel(ulong shares, int duration, double sharePrice)
            {
                this.Shares = shares;
                this.Duration = duration;
                this.SharePrice = sharePrice;
            }

            /// <summary>
            /// Override for use as key in dictionary
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                //https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode
                return (Shares, Duration, SharePrice).GetHashCode();
            }
        }
    }
    
}
