using Microsoft.AspNetCore.Components;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes.Row
{
    public partial class StakeTableRow:ComponentBase
    {
        [Parameter, EditorRequired]
        public BaseStakeDisplayAdaptor Stake { get; set; }
    }
}
