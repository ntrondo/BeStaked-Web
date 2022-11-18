using Microsoft.AspNetCore.Components;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes.Tile
{
    public partial class TStakeTile : ComponentBase, ITransferableStakeComponent
    {
        public TStakeDisplayAdaptor TStake { get => (TStakeDisplayAdaptor)this.Stake; set => this.Stake = value; }
        [Parameter, EditorRequired]
        public BaseStakeDisplayAdaptor Stake { get; set; }
    }
}
