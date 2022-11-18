using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Pages
{
    public partial class Mint : ComponentBase, IDisposable
    {
        [Inject]
        private IExchangeRatesService ExchangeRateService { get; set; }

        [Inject]
        private ILogger<Mint> Logger { get; set; }
        private Shared.Stake.ITypeSelectorModel TypeModel { get; }
        private Shared.Stake.IAmountInput AmountModel { get; }
        private Shared.Stake.IAmountInput DurationModel { get; }
        private Shared.Stake.ISharesEstimate SharesEstimateModel { get; }
        private Shared.Stake.ISubmitButtonModel SubmitButtonModel { get; }

        private Shared.Stake.IAllowanceModel AllowanceModel { get; }
        private Shared.Stake.IBSFeeEstimatesModel FeeEstimateModel { get; }      
        
        public Mint()
        {
            this.TypeModel = new Shared.Stake.TypeSelectorModel();
            this.FeeEstimateModel = new Shared.Stake.BSFeeEstimateModel( this.TypeModel);
            this.AmountModel = new Shared.Stake.AmountInputModelFeeAdjusted(this.FeeEstimateModel);
            this.FeeEstimateModel.ReceiveAmount(this.AmountModel);
            this.DurationModel = new Shared.Stake.DurationInputModel(1, 5555);
            this.AllowanceModel = new Shared.Stake.AllowanceControlModel(this.AmountModel, this.TypeModel);
            this.SharesEstimateModel = new Shared.Stake.SharesEstimateModel(this.AmountModel, this.DurationModel);
            this.SubmitButtonModel = new Shared.Stake.SubmitButtonModel(this.AmountModel, this.AllowanceModel, this.DurationModel, this.TypeModel);
        }
        protected override Task OnInitializedAsync()
        {            
            this.ExchangeRateService.OnExchangeRateLoaded += ExchangeRateService_OnExchangeRateLoaded;
            return base.OnInitializedAsync();
        }

        private void ExchangeRateService_OnExchangeRateLoaded(object sender, EventArgs e)
        {
            //this.Logger.Log(LogLevel.Information, "ExchangeRateService_OnExchangeRateLoaded() factor:" + ExchangeRateService.GetExchangeRate().Factor);
            var exchanger = ExchangeRateService.GetExchangeRate();
            this.FeeEstimateModel.SetExchanger(exchanger);
            this.AmountModel.SetExchanger(exchanger);
            this.SharesEstimateModel.SetExchanger(exchanger);
        }
       
        public void Dispose()
        {
            this.Logger.Log(LogLevel.Information, "Dispose()");
            this.SharesEstimateModel?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
