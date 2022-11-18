using Microsoft.AspNetCore.Components;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes.Tile
{
    public partial class StakeTilePartAmounts : ComponentBase, IStakeComponent
    {
        [Parameter, EditorRequired]
        public BaseStakeDisplayAdaptor Stake { get; set; }
    }
}
