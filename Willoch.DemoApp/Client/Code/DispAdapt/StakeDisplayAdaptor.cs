using System;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Code.DispAdapt
{
    public abstract class BaseStakeDisplayAdaptor : BaseStakesDisplayAdaptor
    {
        private readonly StakeInfo _stake;
        protected BaseStakeDisplayAdaptor(StakeInfo stake, IStakeValuationProvider svp)
            : base(svp)
        {
            this._stake = stake;
            this.StakedAmount = AmountModelFactory.Create(this._stake.StakedAmount);
            this.Shares = AmountModelFactory.Create((double)this._stake.Shares);

            var val = this._stakeValuationProvider.GetEvaluation(stake);
            BookValue = val == null ? this.StakedAmount : AmountModelFactory.Create(val.BookValue);
            DailyInterest = AmountModelFactory.Create(val == null ? 0 : val.DailyInterest);
            MarketValue = val == null ? this.BookValue : AmountModelFactory.Create(val.MarketValue);

            FirstDay = val == null ? default : val.FirstDay;
            LastDay = val == null ? default : val.LastDay;
            IsMature = LastDay > default(DateTime) && LastDay < DateTime.UtcNow.Date;
        }
        public bool IsTransferrable => this._stake is TStakeInfo;
        public string StakeId
        {
            get
            {
                return this._stake.StakeId.ToString();
            }
        }
        public string StakedDays
        {
            get
            {
                return this._stake.StakedDays.ToString();
            }
        }
        public DateTime FirstDay { get; }
        /// <summary>
        /// Last active date of the stake. The day before maturity. default = DateTime.Min
        /// </summary>
        public DateTime LastDay { get; }
        public override IComplexConvertedAmountModel StakedAmount { get; }

        public IComplexAmountModel Shares { get; }
        public override IComplexConvertedAmountModel BookValue { get; }
        public override IComplexConvertedAmountModel MarketValue { get; }
        public IComplexConvertedAmountModel DailyInterest { get; }
        public bool IsMature { get; }
    }
    public class StakeDisplayAdaptor: BaseStakeDisplayAdaptor
    {       

        public StakeDisplayAdaptor(StakeInfo stake, IStakeValuationProvider stakeValuationProvider)
            :base(stake, stakeValuationProvider)
        {           
        }     
    }
    public class TStakeDisplayAdaptor : BaseStakeDisplayAdaptor
    {
        public IComplexConvertedAmountModel Reward { get; }
        public TStakeDisplayAdaptor(StakeInfo stake, IStakeValuationProvider svp) 
            : base(stake, svp)
        {
            var val = this._stakeValuationProvider.GetEvaluation(stake);
            this.Reward = AmountModelFactory.Create(val == null ? 0 : val.Reward);
        }
    }
}
