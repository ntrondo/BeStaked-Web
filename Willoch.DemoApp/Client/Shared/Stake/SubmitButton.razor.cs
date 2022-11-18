using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public interface ISubmitButtonModel
    {
        string Label { get; }
        bool CanStake { get; }
        double Amount { get; }
        ushort Duration { get; }

        Task Stake(ITransferableStakeAccessor accessor/*, ILogger logger*/);

        event EventHandler OnOutputChanged;

    }
    public abstract class SubmitButtonModelBase : ISubmitButtonModel
    {
        public string Label { get; }
        public IAmountInput AmountModel { get; }
        public double Amount => AmountModel.Amount.Amount;
        public IAllowanceModel ApprovalModel { get; }
        public IAmountInput DurationModel { get; }
        private IStakeTypeProvider TypeProvider { get; }

        public ushort Duration => (ushort)DurationModel.Amount.Amount;

        private bool SufficientlyApproved => ApprovalModel.SufficientAllowance;
        private bool HasAmount => AmountModel.Amount.Amount > 0;
        private bool HasDuration => DurationModel.Amount.Amount > 0;
        private Task StakingTask;

        public bool CanStake => HasAmount && SufficientlyApproved && HasDuration;

        protected SubmitButtonModelBase(string label, IAmountInput amountModel, IAllowanceModel approvalModel, IAmountInput durationModel, IStakeTypeProvider typeProvider)
        {
            Label = label;
            this.AmountModel = amountModel;
            this.ApprovalModel = approvalModel;
            this.DurationModel = durationModel;
            this.TypeProvider = typeProvider;

            this.AmountModel.OnAmountChanged += OnInputChanged;
            this.ApprovalModel.OnOutputChanged += OnInputChanged;
            this.DurationModel.OnAmountChanged += OnInputChanged;
        }

        public event EventHandler OnOutputChanged;

        private void OnInputChanged(object sender, System.EventArgs e)
        {
            this.OnOutputChanged?.Invoke(this, EventArgs.Empty);
        }
        public async Task Stake(ITransferableStakeAccessor accessor/*, ILogger logger*/)
        {
            //logger.Log(LogLevel.Information, "SubmitButtonModelBase.Stake() start");
            if (this.StakingTask != null)
                return;
            
            this.StakingTask = accessor.CreateStake(this.Amount, this.Duration);
            //logger.Log(LogLevel.Information, "SubmitButtonModelBase.Stake() created task");
            
            try
            {
                await this.StakingTask;
                //logger.Log(LogLevel.Information, "SubmitButtonModelBase.Stake() task succeeded");
                this.AmountModel.SetAmount(0);
                //logger.Log(LogLevel.Information, "SubmitButtonModelBase.Stake() amount reset");
            }
            catch (Exception)
            {
                //logger.Log(LogLevel.Information, "SubmitButtonModelBase.Stake() task failed");
            }
            this.StakingTask = null;
            //logger.Log(LogLevel.Information, "SubmitButtonModelBase.Stake() end");
        }
    }
    public class SubmitButtonModel : SubmitButtonModelBase
    {
        public SubmitButtonModel(IAmountInput amountModel, IAllowanceModel approvalModel, IAmountInput durationModel, IStakeTypeProvider typeProvider) 
            : base("Create stake", amountModel,approvalModel, durationModel, typeProvider) { }
    }
    public partial class SubmitButton : ComponentBase
    {
        [Parameter, EditorRequired]
        public ISubmitButtonModel Model { get; set; }
        [Inject]
        private ITransferableStakeAccessor TransferableStakeAccessor { get; set; }
        [Inject]
        private ILogger<SubmitButton> logger { get; set; }
        protected override Task OnInitializedAsync()
        {
            this.Model.OnOutputChanged += Model_OnOutputChanged;
            return base.OnInitializedAsync();
        }
        
        private void StakeClicked(MouseEventArgs e)
        {
            _ = this.Model.Stake(this.TransferableStakeAccessor/*, this.logger*/);
        }
        private void Model_OnOutputChanged(object sender, EventArgs e)
        {
            this.StateHasChanged();
        }
    }
}
