using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Code.DispAdapt
{
    public abstract class BaseAdaptor
    {
        public string StakeableSymbol
        {
            get
            {
                return AmountModelFactory.Create(0).Original.Currency.SymbolOrTicker;
            }
        }
        public string FiatSymbol
        {
            get
            {
                return AmountModelFactory.Create(0).Converted.Original.Currency.SymbolOrTicker;
            }
        }
        protected readonly IStakeValuationProvider _stakeValuationProvider;

        public BaseAdaptor( IStakeValuationProvider stakeValuationProvider)
        {
            this._stakeValuationProvider = stakeValuationProvider;
        }
    }
    public abstract class BaseStakesDisplayAdaptor:BaseAdaptor
    {
        public abstract IComplexConvertedAmountModel StakedAmount { get; }
        public abstract IComplexConvertedAmountModel BookValue { get; }
        public abstract IComplexConvertedAmountModel MarketValue { get; }
        public BaseStakesDisplayAdaptor(IStakeValuationProvider stakeValuationProvider)
            :base(stakeValuationProvider)
        { }
    }
}
