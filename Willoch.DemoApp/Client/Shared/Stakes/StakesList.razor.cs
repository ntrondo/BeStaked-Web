using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.DispAdapt;
using Willoch.DemoApp.Client.Code.Models.Amounts;

namespace Willoch.DemoApp.Client.Shared.Stakes
{
    public partial class StakesList:ComponentBase
    {

        [Parameter, EditorRequired]
        public StakesDisplayAdaptor Stakes { get; set; }
        [Parameter, EditorRequired]
        public StakeDisplayMode StakeDisplayMode { get; set; }
        [Parameter]
        public bool IsExpanded { get; set; } = true;
        public StakeType StakeType => Stakes.StakeType;
        private Type _stakeTileType;
        private Type StakeTileType
        {
            get
            {
                if(this._stakeTileType == null)
                    this._stakeTileType = this.StakeType switch
                    {
                        StakeType.Transferable => typeof(Tile.TStakeTile),
                        _ => typeof(Tile.StakeTile),
                    };
                return this._stakeTileType;
            }
        }
        private static Dictionary<string, object> WrapKeyValue(string key, object value)
        {
            var dict = new Dictionary<string, object>
            {
                { key, value }
            };
            return dict;
        }
        private IComplexConvertedAmountModel Sum
        {
            get
            {
                return this.StakeType switch
                {
                    StakeType.Legacy => this.Stakes.BookValue,
                    _ => this.Stakes.MarketValue,
                };
            }
        }
        private static readonly IConvert<StakeType, string> StakeTypeToStringConverter = new StakeTypeToPluralDisplayStringConverter();
    
        
        private void Collapse()
        {
            IsExpanded = false;
        }
        private void Expand()
        {
            this.IsExpanded = true;
        }
        private void DisplayTable()
        {
            this.StakeDisplayMode = StakeDisplayMode.Table;
        }
        private void DisplayTiles()
        {
            this.StakeDisplayMode = StakeDisplayMode.Tiles;
        }
    }
}
