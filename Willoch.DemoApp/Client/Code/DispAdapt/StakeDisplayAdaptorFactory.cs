using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Code.DispAdapt
{
   
    public abstract class BaseStakeDisplayAdaptorFactory<T> : BaseAdaptor where T : BaseStakesDisplayAdaptor
    {
        public BaseStakeDisplayAdaptorFactory(IStakeValuationProvider svp) 
            : base(svp)
        {
        }
        public abstract T CreateDisplayAdaptor(StakeInfo stake);
    }
    public class StakeDisplayAdaptorFactory: BaseStakeDisplayAdaptorFactory<StakeDisplayAdaptor>
    {
        public StakeDisplayAdaptorFactory(IStakeValuationProvider stakeValuationProvider)
        :base(stakeValuationProvider)
        { }

        public override StakeDisplayAdaptor CreateDisplayAdaptor(StakeInfo stake)
        {
            var adaptor = new StakeDisplayAdaptor(stake, _stakeValuationProvider);
            return adaptor;
        }
    }
    public class TransferableStakeAdaptorFactory : BaseStakeDisplayAdaptorFactory<TStakeDisplayAdaptor>
    {
        public TransferableStakeAdaptorFactory(IStakeValuationProvider svp) 
            : base(svp)
        {
        }
        public override TStakeDisplayAdaptor CreateDisplayAdaptor(StakeInfo stake)
        {
            var adaptor = new TStakeDisplayAdaptor(stake, _stakeValuationProvider);
            return adaptor;
        }
    }
}
