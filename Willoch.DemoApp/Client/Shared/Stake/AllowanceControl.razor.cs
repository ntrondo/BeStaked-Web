using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stakes;
using Willoch.DemoApp.Shared.Utilities;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public interface IAllowanceModel
    {
        string Label { get; }
        string Symbol { get; }
        bool SufficientAllowance { get; }
        bool AnyAllowance { get; }
        IAmountInput RequiredAllowance { get; }
        string RequiredAmountString { get; }
        IExposeAmountAsString Allowance { get; }
        bool AllowanceRequired { get; }
        void SetAllowance(double amount, StakeType stakeType);
        event EventHandler OnOutputChanged;
    }
    public abstract class BaseAllowanceControlModel: IAllowanceModel
    {        
        public abstract string Label { get; }
        protected IConvert<double,string> AmountFormatter = UtilitiesLib.ConvertPrimitives.Implementations.Double.ConstrainedDoubleAmountToShortStringConverter.Instance;
        protected BaseAllowanceControlModel(IAmountInput requiredAmount, IStakeTypeProvider typeSelector)
        {
            RequiredAllowance = requiredAmount;
            this.TypeSelector = typeSelector;
            this.RequiredAllowance.OnAmountChanged += RequiredAmount_OnAmountChanged;
            this.TypeSelector.OnValueChanged += TypeSelector_OnValueChanged;
            foreach (var stakeType in typeSelector.Values)
                this.Allowances.Add(stakeType, new AmountAsStringModel(0, AmountFormatter));           
        }
        public event EventHandler OnOutputChanged;
        private void RequiredAmount_OnAmountChanged(object sender, System.EventArgs e)
        {
            this.OnOutputChanged?.Invoke(sender, e);
        }
        private void TypeSelector_OnValueChanged(object sender, EventArgs e)
        {
            this.OnOutputChanged?.Invoke(sender, e);
        }
        public void SetAllowance(double amount, StakeType stakeType)
        {
            if (this.Allowances[stakeType].Amount != amount)
            {
                this.Allowances[stakeType] = new AmountAsStringModel(amount, AmountFormatter);
                this.OnOutputChanged?.Invoke(this, null);
            }
        }

        public string Symbol => this.RequiredAllowance.Symbol;
        private readonly Dictionary<StakeType, IExposeAmountAsString> Allowances = new();
        public IExposeAmountAsString Allowance { get => Allowances[TypeSelector.Value]; }
        public IAmountInput RequiredAllowance { get; }
        public IStakeTypeProvider TypeSelector { get; }

        public string RequiredAmountString => AmountFormatter.Convert(RequiredAllowance.Amount.Amount);
        public bool SufficientAllowance => !AllowanceRequired || Allowance.Amount >= RequiredAllowance.Amount.Amount;
        public bool AnyAllowance => AllowanceRequired && Allowance.Amount > 0;

        public bool AllowanceRequired => TypeSelector.Value != StakeType.Legacy;
    }
    public class MockAllowanceControlModel:BaseAllowanceControlModel
    {
        public MockAllowanceControlModel(double allowance, IAmountInput requiredAmount, ITypeSelectorModel typeSelector)
            :base(requiredAmount, typeSelector)
        {
            foreach (var stakeType in typeSelector.Values)
                this.SetAllowance(allowance, stakeType);
        }

        public override string Label => "Mock allowance";
    }
    public class AllowanceControlModel : BaseAllowanceControlModel
    {
        public AllowanceControlModel(IAmountInput requiredAmount, IStakeTypeProvider typeSelector)
            : base(requiredAmount, typeSelector)
        {}

        public override string Label => "Allowance";
    }
    public partial class AllowanceControl : ComponentBase
    {
        [Parameter, EditorRequired]
        public IAllowanceModel Model { get; set; }
        [Inject]
        private ILogger<AllowanceControl> logger { get; set; }
        [Inject]
        private ITransferableStakeAccessor TransferableStakeAsyncAccessor { get; set; }
        public bool ExplicitlyExpanded { get; set; }
        public bool IsExpanded => ExplicitlyExpanded || !Model.SufficientAllowance;
        public bool CanCollapse => Model.SufficientAllowance;
        private void Collapse()
        {
            this.ExplicitlyExpanded = false;
        }
        private void Expand()
        {
            this.ExplicitlyExpanded = true;
        }
        protected override Task OnInitializedAsync()
        {
            this.Model.OnOutputChanged += Model_OnOutputChanged;
            this.LoadAllowance();
            return base.OnInitializedAsync();
        }

        private async void LoadAllowance()
        {
            this.Model.SetAllowance(double.MaxValue, StakeType.Legacy);
            double amount = await this.TransferableStakeAsyncAccessor.GetStakeableAllowanceAsync();
            //this.logger.Log(LogLevel.Information, "LoadAllowance() amount=" + amount);            
            this.Model.SetAllowance(amount, StakeType.Transferable);
        }

        private void Model_OnOutputChanged(object sender, EventArgs e)
        {
            this.StateHasChanged();
        }
        private Task<bool> ApprovingTask = null;
        private async void ClearAllowanceClicked()
        {
            await SetAllowance(0);
        }
        private async void ApproveExactClicked()
        {
            await SetAllowance(this.Model.RequiredAllowance.Amount.Amount);
        }
        private async void ApproveInfiniteClicked()
        {
            await SetAllowance(double.MaxValue);
        }
        private async Task SetAllowance(double amount)
        {
            if (this.ApprovingTask != null)
                return;
            this.logger.Log(LogLevel.Information, "ClearAllowanceClicked()");
            this.ApprovingTask = TransferableStakeAsyncAccessor.ApproveStakeable(amount);
            var result = await this.ApprovingTask;
            this.ApprovingTask = null;
        }
    }
}
