using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.Models.Implementations;
using UtilitiesLibBeStaked.Factories;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public class DurationInputModel : AmountInputModelBase
    {
        public override string Symbol { get; } = "Days";
        public DurationInputModel(double min, double max) 
            : base("Duration", min, max)
        {
            SetAmount(max);
        }        

        public override IComplexAmountModel Amount { get => _amount; }
        private IComplexAmountModel _amount;
        public override event EventHandler OnAmountChanged;
        public override void SetAmount(double amount)
        {
            bool isChanged = this._amount != null && this._amount.Amount != amount;
            this._amount = AmountModelFactory.Create(amount);
            if (isChanged)
                this.OnAmountChanged?.Invoke(this, null);
        }
    }
   
    public partial class DurationInput:NumericInputComponentBase
    {     
        [Inject]
        private ILogger<DurationInput> Logger { get; set; }
        protected override void Log(string message)
        {
            Logger.Log(LogLevel.Information, message);
        }
        private DateTime? Date
        {
            get 
            {
                if (this.Model.Amount.Amount < this.Model.Minimum.Amount)
                    return null;
                return TomorrowUTC.AddDays(this.Model.Amount.Amount);
            }
            set
            {
                Log("Date set() " + value == null ? "null" : value.Value.ToString());
                TimeSpan duration = value == null ? new TimeSpan(0) : (value.Value.Date - TomorrowUTC);
                Log("Date set() duration:" + duration.TotalDays);
                this.InputValue = ((int)duration.TotalDays).ToString();
            }
        }
        private static readonly int DaysPerYear = 365;

        private static readonly DateTime TodayUTC = DateTime.UtcNow.Date, TomorrowUTC = TodayUTC.AddDays(1);
        private static readonly IConvert<DateTime, string> DateToDatePickerStringConverter = new  DateToDatePickerStringConverter();
        private string MinimumDateString => DateToDatePickerStringConverter.Convert(TomorrowUTC.AddDays(Model.Minimum.Amount));
        private string MaximumDateString { get => DateToDatePickerStringConverter.Convert(TomorrowUTC.AddDays(Model.Maximum.Amount)); }
        
        

        protected override RelativeAmountOptionModel[] GenerateOptions()
        {
            var max = (int)this.Model.Maximum.Amount;
            //IConvert<double, string> dblFormatter = new DoubleToLongStringConverter();
            ITransform<double> rounder = new DoubleRounder(0, RoundingMode.TowardsZero);
            var list = new List<RelativeAmountOptionModel>();
            list.Add(new RelativeAmountOptionModel("Maximum", AmountModelFactory.Create(max)));
            for (int i = max / DaysPerYear; i > 0; i--)
            {
                string label = string.Format("{0} {1}", i, i == 1 ? "year" : "years");
                double amount = rounder.Convert((TodayUTC.AddYears(i) - TodayUTC).TotalDays);
                if (amount >= this.Model.Maximum.Amount)
                    continue;
                if (amount <= this.Model.Minimum.Amount)
                    break;
                list.Add(new RelativeAmountOptionModel(label, AmountModelFactory.Create(amount)));
            }
            {
                double lpb = 3641.0;
                string label = string.Format("{0} {1}", Math.Round(lpb / DaysPerYear, 2), "years (Bonus max)");
                list.Add(new RelativeAmountOptionModel(label, AmountModelFactory.Create(lpb)));
            }            
            return list.OrderByDescending(o=>o.Amount.Amount).ToArray();
        }
    }
}
