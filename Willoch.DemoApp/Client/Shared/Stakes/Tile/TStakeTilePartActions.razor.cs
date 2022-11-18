using Microsoft.AspNetCore.Components;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes.Tile
{
    public partial class TStakeTilePartActions : ComponentBase, ITransferableStakeComponent, IStakeActionsComponent
    {
        [Parameter, EditorRequired]
        public BaseStakeDisplayAdaptor Stake { get; set; }
        public TStakeDisplayAdaptor TStake { get => (TStakeDisplayAdaptor)this.Stake; set => this.Stake = value; }

        public void EndStakeClicked()
        {
            throw new System.NotImplementedException();
        }
    }
}
