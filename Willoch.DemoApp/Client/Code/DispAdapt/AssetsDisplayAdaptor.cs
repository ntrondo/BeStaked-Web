using System.Collections.Generic;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Code.DispAdapt
{
    public class AssetsDisplayAdaptor : BaseAdaptor
    {
        private readonly AssetsModel _assets;

        public AssetsDisplayAdaptor(AssetsModel assets, IStakeValuationProvider stakeValuationProvider) 
            : base(stakeValuationProvider)
        {
            this._assets = assets;      
            this.Balance = AmountModelFactory.Create(assets.StakeableBalance);
        }
        public IComplexConvertedAmountModel Balance { get; }
        private IComplexConvertedAmountModel _sum;
        public IComplexConvertedAmountModel Sum 
        {
            get
            {
                if(this._sum == null)
                {
                    double amount = this.Balance.Amount;
                    foreach (var stakeType in this._assets.StakeTypes)
                        amount += this.GetSum(stakeType).Amount;
                    this._sum = AmountModelFactory.Create(amount);
                }
                return this._sum;
            }
        }
        private readonly Dictionary<StakeType, IComplexConvertedAmountModel> _sumsByStakeType = new();
        public IComplexConvertedAmountModel GetSum(StakeType stakeType)
        {
            if(this._sumsByStakeType.ContainsKey(stakeType))
                return this._sumsByStakeType[stakeType];
            var adaptor = this[stakeType];
            IComplexConvertedAmountModel sum = stakeType switch
            {
                StakeType.Legacy => adaptor.BookValue,
                _ => adaptor.MarketValue,
            };
            this._sumsByStakeType.Add(stakeType, sum);
            return sum;
        }
        private StakesDisplayAdaptorFactory _adaptorFactory;
        public StakesDisplayAdaptorFactory AdaptorFactory 
        {
            get
            {
                if (this._adaptorFactory == null)
                    this._adaptorFactory = new StakesDisplayAdaptorFactory(_stakeValuationProvider);
                return this._adaptorFactory;
            }
        }
        private readonly Dictionary<StakeType, StakesDisplayAdaptor> _adaptorsByStakeType = new();
        public StakesDisplayAdaptor this[StakeType stakeType]
        {
            get
            {
                if (_adaptorsByStakeType.ContainsKey(stakeType))
                    return _adaptorsByStakeType[stakeType];
                var stakes = this._assets[stakeType];
                var adaptor = this.AdaptorFactory.CreateDisplayAdaptor(stakes, stakeType);
                this._adaptorsByStakeType.Add(stakeType, adaptor);
                return adaptor;
            }
        }
        
    }
}
