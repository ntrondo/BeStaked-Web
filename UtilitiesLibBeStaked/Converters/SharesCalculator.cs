using System;
using System.Collections.Generic;
using UtilitiesLib.ConvertPrimitives.BaseClasses;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace UtilitiesLibBeStaked.Converters
{
    public interface ICalculateShares
    {
        SharesCalculationResult GetResult(double amount, int duration);
    }
    public class SharesCalculationResult
    {
        
    }
    public class SharesCalculator : CachedConverterBase<SharesCalculator.InputModel, SharesCalculator.OutputModel>
    {
        public double SharePrice { get; }
        private readonly Dictionary<Tuple<double, int>, SharesCalculationResult> _cache = new();
       
        public SharesCalculator(double sharePrice) 
        {
            this.SharePrice = sharePrice;
        }
        //public SharesCalculationResult GetResult(double amount, int duration)
        //{
        //    var key = new Tuple<double, int>(amount, duration);
        //    if(!_cache.ContainsKey(key))
        //    {
        //        //https://hexicans.info/documentation/deep-dive/#rules     
        //        double bonusAmount = BonusAmountCalculator.Instance.GetResult(amount, duration);
        //        double effectiveAmount = amount + bonusAmount;
        //        double shares = effectiveAmount / this.SharePrice;
        //        _cache.Add(key, new SharesCalculationResult(amount,bonusAmount, duration, shares));
        //    }
        //    return _cache[key];
        //}

        public override OutputModel convert(InputModel value)
        {
            //https://hexicans.info/documentation/deep-dive/#rules     
            double bonusAmount = BonusAmountCalculator.Instance.Convert(value);
            double effectiveAmount = value.Principal + bonusAmount;
            double shares = effectiveAmount / this.SharePrice;
            return new OutputModel(value, bonusAmount, shares);
        }

        public class InputModel
        {
            public double Principal { get; }
            public int Duration { get; }
            public InputModel(double principal, int duration)
            {
                Principal = principal;
                Duration = duration;
            }
            public override int GetHashCode()
            {
                return (Principal, Duration).GetHashCode();
            }
        }
        public class OutputModel
        {
            public OutputModel(InputModel input, double bonusAmount, double shares)
            {
                this.Input = input;
                BonusAmount = bonusAmount;
                Shares = shares;
            }
            private InputModel Input { get; }
            public double BaseAmount => Input.Principal;
            public double BonusAmount { get; }
            public double TotalAmount { get => BaseAmount + BonusAmount; }
            public int Duration => Input.Duration;
            public double Shares { get; }
            public double EffectiveSharePrice => BaseAmount / Shares;
        }
    }
}
