using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Code.DispAdapt
{
    public class StakesDisplayAdaptorFactory:BaseAdaptor
    {
        public StakesDisplayAdaptorFactory(Services.IStakeValuationProvider stakeValuationProvider)
            :base(stakeValuationProvider)
        {
        }
        public StakesDisplayAdaptor CreateDisplayAdaptor(StakeInfo[] stakes, StakeType stakeType)
        {
            return new StakesDisplayAdaptor(stakes, stakeType, this._stakeValuationProvider);
        }
    }
}
