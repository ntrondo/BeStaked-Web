using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Converters;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public interface ISharesEstimate: IDisposable
    {
        string Label { get; }
        IComplexConvertedAmountModel SharePrice { get; }
        IComplexConvertedAmountModel EffectiveSharePrice { get; }
        IComplexAmountModel TotalShares { get; }
        IComplexConvertedAmountModel BaseAmount { get; }
        IComplexConvertedAmountModel BonusAmount { get; }
        IComplexConvertedAmountModel TotalAmount { get; }
        //IConvertDoubleAndKnowTickers CurrencyConverter { get; }
        string PrimarySymbol { get; }
        string ForeignSymbol { get; }
        void SetExchanger(IConvert<double, ICurrencyAmount> exchanger);
        void SetSharePrice(double sharePrice);
        event EventHandler OnOutputChanged;
    }
    internal abstract class BaseSharesEstimateModel : ISharesEstimate
    {
        public string Label { get; }
        private IAmountInput AmountModel { get; }
        private IAmountInput DurationModel { get; }
        private IConvert<double, ICurrencyAmount> CurrencyConverter { get;  set; }
        public IComplexConvertedAmountModel SharePrice { get; private set; }
        public IComplexConvertedAmountModel EffectiveSharePrice { get; private set; }

        private SharesCalculator SharesCalculator { get; set; }
        public IComplexAmountModel TotalShares { get;private set; }
        public IComplexConvertedAmountModel BaseAmount { get; private set; }
        public IComplexConvertedAmountModel BonusAmount { get; private set; }
        public IComplexConvertedAmountModel TotalAmount { get; private set; }
        private SharesCalculator.OutputModel SharesCalculationResult { get; set; }

        public string PrimarySymbol => BaseAmount.Original.Currency.SymbolOrTicker;

        public string ForeignSymbol => BaseAmount.Converted.Original.Currency.SymbolOrTicker;

        static readonly IConvert<double, string> AmountToStringConverter = ConstrainedDoubleAmountToShortStringConverter.Instance;
        static readonly IConvert<double, string> SharePriceToStringConverter = new SharePriceToShortDisplayStringConverter();
        protected BaseSharesEstimateModel(string label, IAmountInput amountModel, IAmountInput durationModel)
        {
            Label = label;
            this.AmountModel = amountModel;
            this.DurationModel = durationModel;

            this.AmountModel.OnAmountChanged += OnAmountOrDurationChanged;
            this.DurationModel.OnAmountChanged += OnAmountOrDurationChanged;
        }
        private void OnAmountOrDurationChanged(object sender, EventArgs e)
        {
            //logger?.Log(LogLevel.Information, "Willoch.DemoApp.Client.Shared.Stake.BaseSharesEstimateModel.OnAmountOrDurationChanged()");         
            if (this.SharesCalculator != null)
                this.SharesCalculationResult = this.SharesCalculator.Convert(new(this.AmountModel.Amount.Amount, (int)this.DurationModel.Amount.Amount));
            if(SharesCalculationResult != null)
            {
                this.TotalShares = AmountModelFactory.Create(this.SharesCalculationResult.Shares);
                this.EffectiveSharePrice = AmountModelFactory.Create(this.SharesCalculationResult.EffectiveSharePrice);
            }
                
            this.ResetAmounts();
            this.OnOutputChanged?.Invoke(this, e);
        }    
        private void ResetAmounts()
        {
            if (SharesCalculationResult != null)
            {
                var baseAmount = this.SharesCalculationResult.BaseAmount;
                var bonus = this.SharesCalculationResult.BonusAmount;
                var tot = this.SharesCalculationResult.TotalAmount;
                var asc = AmountToStringConverter;
                var cc = this.CurrencyConverter;
                this.BaseAmount = AmountModelFactory.Create(baseAmount);
                this.BonusAmount = AmountModelFactory.Create(bonus);
                this.TotalAmount = AmountModelFactory.Create(tot);
            }
        }

        public void Dispose()
        {
            this.AmountModel.OnAmountChanged -= OnAmountOrDurationChanged;
            this.DurationModel.OnAmountChanged -= OnAmountOrDurationChanged;            
        }

        public void SetExchanger(IConvert<double, ICurrencyAmount> exchanger)
        {
            if(this.CurrencyConverter != exchanger)
            {
                this.CurrencyConverter = exchanger;
                this.ResetAmounts();
                if (SharesCalculationResult != null)
                    this.OnOutputChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetSharePrice(double sharePrice)
        {
            double oldSharePrice = this.SharePrice == null ? default : this.SharePrice.Amount;
            if (oldSharePrice != sharePrice)
            {
                this.SharePrice = AmountModelFactory.Create(sharePrice);
                this.SharesCalculator = new SharesCalculator(this.SharePrice.Amount);
                OnAmountOrDurationChanged(this, null);
            }
        }

        public event EventHandler OnOutputChanged;           
    }
    internal class MockSharesEstimateModel :BaseSharesEstimateModel
    {
        public MockSharesEstimateModel(IAmountInput amountModel, IAmountInput durationModel)
            :base("Mock shares estimate", amountModel, durationModel)
        {
        }
    }
    internal class SharesEstimateModel : BaseSharesEstimateModel
    {
        public SharesEstimateModel(IAmountInput amountModel, IAmountInput durationModel)
            : base("Shares", amountModel, durationModel)
        {
        }
    }
    public partial class SharesEstimate:ComponentBase,IDisposable
    {
        [Parameter, EditorRequired]
        public ISharesEstimate Model { get; set; }
        [Inject]
        private IStakeableAsyncAccessor StakeableAsyncAccessor { get; set; }
        [Inject]
        private ILogger<SharesEstimate> logger { get; set; }

        protected override Task OnInitializedAsync()
        {            
            this.Model.OnOutputChanged += Model_OnOutputChanged;
            this.LoadSharePrice();            
            return base.OnInitializedAsync();
        }

        private async void LoadSharePrice()
        {
            //logger.Log(LogLevel.Information, "LoadSharePrice()");
            var sp = await this.StakeableAsyncAccessor.GetSharePriceAsync();
            this.Model.SetSharePrice(sp);
        }

        private void Model_OnOutputChanged(object sender, EventArgs e)
        {
            logger.Log(LogLevel.Information, "Model_OnInputChanged()");
            this.StateHasChanged();
        }

        public void Dispose()
        {
            this.Model.OnOutputChanged -= Model_OnOutputChanged;
            GC.SuppressFinalize(this);
        }
    }
}
