using Microsoft.AspNetCore.Components;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes.Tile
{
    public partial class StakeTilePartActions : ComponentBase, IStakeActionsComponent
    {
        [Parameter, EditorRequired]
        public BaseStakeDisplayAdaptor Stake { get; set; }

        public void EndStakeClicked()
        {
            throw new System.NotImplementedException();
        }
    }
}
