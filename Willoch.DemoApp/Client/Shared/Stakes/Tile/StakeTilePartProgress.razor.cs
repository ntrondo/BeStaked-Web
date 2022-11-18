using Microsoft.AspNetCore.Components;
using System;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.DispAdapt;

namespace Willoch.DemoApp.Client.Shared.Stakes.Tile
{
    public partial class StakeTilePartProgress : ComponentBase, IStakeComponent
    {
        private string FirstDayString => DateToDisplayString(Stake.FirstDay);
        private string MaturityDayString => DateToDisplayString(Stake.LastDay.AddDays(1));
        
        private string ProgressString
        {
            get
            {
                if (DateToProgressConverter == null)
                    return string.Empty;
                return new DateProgressToPercentageStringConverter(DateToProgressConverter).Convert(DateTime.UtcNow);
            }
        }
        private static string DateToDisplayString(DateTime dt)
        {
            if (dt.Year == 1)
                return string.Empty;
            return dt.ToShortDateString();
        }
        private string RemainingString => DateConverter.Convert(Stake.LastDay.AddDays(1));

        [Parameter, EditorRequired]
        public BaseStakeDisplayAdaptor Stake { get; set; }

        private static readonly IConvert<DateTime, string> DateConverter = new DateToRelativeStringConverter();
        DateProgressPercentageConverter _dateToProgressConverter;
        DateProgressPercentageConverter DateToProgressConverter
        {
            get
            {
                if (this._dateToProgressConverter == null)
                {
                    if (Stake.FirstDay != default && Stake.LastDay != default)
                        this._dateToProgressConverter = new DateProgressPercentageConverter(Stake.FirstDay, Stake.LastDay);
                }
                return this._dateToProgressConverter;
            }
        }
        private int Progress
        {
            get
            {
                if (DateToProgressConverter == null)
                    return default;
                return (int)DateToProgressConverter.Convert(DateTime.UtcNow);
            }
        }
    }
}
