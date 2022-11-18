using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models.Amounts;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public interface IAmountInput
    {
        string Label { get; }
        string Symbol { get; }
        IConvert<double, ICurrencyAmount> Exchanger { get; }
        IComplexAmountModel Minimum { get; }
        IComplexAmountModel Maximum { get; }
        double Range { get; }
        IComplexAmountModel Amount { get; }
        void SetAmount(double amount);
        void SetRange(double min, double max);
        void SetExchanger(IConvert<double, ICurrencyAmount> exchanger);
        event EventHandler OnAmountChanged;
        event EventHandler OnRangeChanged;
    }
    public abstract class AmountInputModelBase : IAmountInput
    {
        public string Label { get; }
        public abstract string Symbol { get; }
        public IConvert<double, ICurrencyAmount> Exchanger { get; private set; }
        public abstract IComplexAmountModel Amount { get; }
        public IComplexAmountModel Minimum { get; private set; }        
        public IComplexAmountModel Maximum { get; private set; }
        public double Range => Maximum.Amount - Minimum.Amount;
        
        public AmountInputModelBase(string label, double min, double max)
        {
            this.Label = label;
            this.Minimum = AmountModelFactory.Create(min);
            this.Maximum = AmountModelFactory.Create(max);
            this.OnRangeChanged += AmountInputModelBase_OnRangeChanged;
        }
        /// <summary>
        /// Adjusts amount if it is out of permitted range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AmountInputModelBase_OnRangeChanged(object sender, EventArgs e)
        {
            if (this.Amount == null)
                return;
            if (this.Amount.Amount < this.Minimum.Amount)
                this.SetAmount(this.Minimum.Amount);
            else if (this.Amount.Amount > this.Maximum.Amount)
                this.SetAmount(this.Maximum.Amount);
        }

        public abstract void SetAmount(double amount);
        public virtual void SetRange(double min, double max)
        {
            bool changed = false;
            if(this.Minimum == null || this.Minimum.Amount != min)
            {
                this.Minimum = AmountModelFactory.Create(min);
                changed = true;
            }
            if (this.Maximum == null || this.Maximum.Amount != max)
            {
                this.Maximum = AmountModelFactory.Create(max);
                changed = true;
            }
            if (changed)
                this.OnRangeChanged?.Invoke(this, EventArgs.Empty);
        }
        public void SetExchanger(IConvert<double, ICurrencyAmount> exchanger)
        {
            if(this.Exchanger != exchanger)
            {
                this.Exchanger = exchanger;
                this.ExchangerChanged();
            }
        }
        protected virtual void ExchangerChanged() {}

        public abstract event EventHandler OnAmountChanged;
        public event EventHandler OnRangeChanged;
    }
    public class RelativeAmountOptionModel
    {
        public string Label { get; }
        public IComplexAmountModel Amount { get; }
        public RelativeAmountOptionModel(string label, IComplexAmountModel amount)
        {
            this.Label = label;
            this.Amount = amount;
        }
    }
    public abstract partial class NumericInputComponentBase:ComponentBase
    {
        [Parameter, EditorRequired]
        public IAmountInput Model { get; set; }
        protected static readonly IConvert<string, double> InputParser = new DoubleParser();
        protected string InputValue
        {
            get => this.Model.Amount.Original.AmountString;
            set
            {
                double newAmount = InputParser.Convert(value);
                if (newAmount == 0)
                    Log(string.Format("InputValue_set() => DoubleParser.Convert({0}) => {1}", value, newAmount));
                if (newAmount == Model.Amount.Amount)
                    return;
                newAmount = Math.Min(newAmount, Model.Maximum.Amount);
                newAmount = Math.Max(newAmount, Model.Minimum.Amount);
                
                Model.SetAmount(newAmount);
            }
        }
        private string selectedOption;
        public string SelectedOption
        {
            get => selectedOption;
            set
            {
                this.selectedOption = value;
                this.OnSelectedAmountOptionChange();
            }
        }
        private RelativeAmountOptionModel[] _relativeAmountOptions = Array.Empty<RelativeAmountOptionModel>();
        protected RelativeAmountOptionModel[] RelativeAmountOptions
        {
            get
            {
                if (this._relativeAmountOptions.Length == 0 && Model.Range > 0)
                    this._relativeAmountOptions = this.GenerateOptions();
                return this._relativeAmountOptions;
            }
        }
        protected override Task OnInitializedAsync()
        {
            this.Model.OnRangeChanged += Model_OnRangeChanged;
            this.Model.OnAmountChanged += Model_OnAmountChanged;
            this.AdjustSelectedOptionToAmount();
            return base.OnInitializedAsync();
        }

        private void Model_OnAmountChanged(object sender, EventArgs e)
        {
            this.AdjustSelectedOptionToAmount();
            base.StateHasChanged();
        }

        private void Model_OnRangeChanged(object sender, EventArgs e)
        {
            this._relativeAmountOptions = Array.Empty<RelativeAmountOptionModel>();
            this.selectedOption = null;
            base.StateHasChanged();
        }
        private void AdjustSelectedOptionToAmount()
        {
            var option = RelativeAmountOptions.FirstOrDefault(o => o.Amount.Amount == this.Model.Amount.Amount);
            selectedOption = option?.Label;
        }
        protected abstract RelativeAmountOptionModel[] GenerateOptions();


        void OnSelectedAmountOptionChange()
        {
            Log("OnSelectedAmountOptionChange() option:" + SelectedOption);
            var option = this.RelativeAmountOptions.FirstOrDefault(o => o.Label == this.SelectedOption);
            if (option == null)
                return;
            this.Model.SetAmount(option.Amount.Amount);
        }
        protected abstract void Log(string message);
    }
}
