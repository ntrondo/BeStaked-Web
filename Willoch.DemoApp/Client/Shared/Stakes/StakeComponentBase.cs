using Microsoft.AspNetCore.Components;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes
{
    public interface IStakeComponent
    {
        [Parameter, EditorRequired]
        BaseStakeDisplayAdaptor Stake { get; set; }
    }
    public interface ILegacyStakeComponent:IStakeComponent
    {
        StakeDisplayAdaptor LStake { get; set; }
    }
    public interface ITransferableStakeComponent:IStakeComponent
    {
        TStakeDisplayAdaptor TStake { get; set; }
    }
    public interface IStakeActionsComponent:IStakeComponent
    {
        void EndStakeClicked();
    } 
}
