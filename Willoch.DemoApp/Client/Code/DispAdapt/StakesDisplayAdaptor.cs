using System.Collections.Generic;
using System.Linq;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Code.DispAdapt
{

    public class StakesDisplayAdaptor : BaseStakesDisplayAdaptor
    {
        private readonly StakeInfo[] _stakes;
        public StakeType StakeType { get; }

        public StakesDisplayAdaptor(StakeInfo[] stakes, StakeType stakeType, IStakeValuationProvider stakeValuationProvider) 
            : base(stakeValuationProvider)
        {
            this._stakes = stakes;
            this.StakeType = stakeType;
        }
        public IEnumerable<BaseStakeDisplayAdaptor> StakeAdaptors
        {
            get
            {
                if(this.StakeType == StakeType.Transferable)
                {
                   
                    var f = new TransferableStakeAdaptorFactory(base._stakeValuationProvider);
                    return this._stakes.Select(s => f.CreateDisplayAdaptor(s));
                }
                else
                {
                    var f = new StakeDisplayAdaptorFactory(base._stakeValuationProvider);
                    return this._stakes.Select(s => f.CreateDisplayAdaptor(s));
                }         
            }
        }
        private IComplexConvertedAmountModel _stakedAmount;
        public override IComplexConvertedAmountModel StakedAmount
        {
            get
            {
                if(this._stakedAmount == null)
                {
                    var amount = this._stakes.Sum(s => s.StakedAmount);
                    this._stakedAmount = AmountModelFactory.Create(amount);
                }
                return this._stakedAmount;
            }
        }
        private IComplexConvertedAmountModel _bookValue;
        public override IComplexConvertedAmountModel BookValue
        {
            get
            {
                if (this._bookValue == null)
                {
                    double amount = 0;
                    StakeValuation val;
                    foreach(var stake in this._stakes)
                    {
                        val = this._stakeValuationProvider.GetEvaluation(stake);
                        amount += val == null ? stake.StakedAmount : val.BookValue;
                    }
                    this._bookValue = AmountModelFactory.Create(amount);
                }
                return this._bookValue;
            }
        }
        private IComplexConvertedAmountModel _marketValue;
        public override IComplexConvertedAmountModel MarketValue
        {
            get
            {
                if (this._marketValue == null)
                {
                    double amount = 0;
                    StakeValuation val;
                    foreach (var stake in this._stakes)
                    {
                        val = this._stakeValuationProvider.GetEvaluation(stake);
                        amount += val == null ? stake.StakedAmount : val.MarketValue;
                    }
                    this._marketValue = AmountModelFactory.Create(amount);
                }
                return this._marketValue;
            }
        }
    }
}
