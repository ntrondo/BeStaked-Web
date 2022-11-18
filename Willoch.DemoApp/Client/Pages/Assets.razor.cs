using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Code.DispAdapt;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Pages
{
    public partial class Assets : ComponentBase, IDisposable
    {
        [Inject]
        private IAssetsService AssetsService { get; set; }
        [Inject]
        private IExchangeRatesService ExchangeRateService { get; set; }
        [Inject]
        private IStakeValuationService ValuationService { get; set; }
        private AssetsModel Stakes
        {
            get
            {
                if (AssetsService?.WalletAssets == null)
                    return new AssetsModel();
                return AssetsService.WalletAssets;
            }
        }
       
        private StakesDisplayAdaptorFactory AdaptorFactory { get; set; }

        private Shared.Stakes.StakeDisplayMode StakeDisplayMode { get; set; } = Shared.Stakes.StakeDisplayMode.Tiles;

        protected override Task OnInitializedAsync()
        {
            this.ExchangeRateService.OnExchangeRateLoaded += ExchangeRateService_OnExchangeRateLoaded;
            this.AssetsService.DataRefreshed += AssetsService_DataRefreshed;
            this.ValuationService.OnValuationProgress += ValuationService_OnValuationProgress;
            this.ValuationService.OnValuationCompleted += ValuationService_OnValuationCompleted;
            this.RecreateFactory();
            return base.OnInitializedAsync();
        }

        private void ValuationService_OnValuationCompleted(object sender, EventArgs e)
        {
            InvokeAsync(this.StateHasChanged);
        }

        private void ValuationService_OnValuationProgress(object sender, EventArgs e)
        {
            InvokeAsync(this.StateHasChanged);
        }

        private void AssetsService_DataRefreshed(object sender, EventArgs e)
        {
            this.RecreateFactory();
        }
        private void ExchangeRateService_OnExchangeRateLoaded(object sender, EventArgs e)
        {
            this.RecreateFactory();
        }

        private void RecreateFactory()
        {            
            IConvert<double, string> stringConverter = ConstrainedDoubleAmountToShortStringConverter.Instance;
            this.AdaptorFactory = new StakesDisplayAdaptorFactory(this.ValuationService);
            InvokeAsync(this.StateHasChanged);
        }

        
        public void Dispose()
        {
            this.AssetsService.DataRefreshed -= AssetsService_DataRefreshed;
            this.ExchangeRateService.OnExchangeRateLoaded-= ExchangeRateService_OnExchangeRateLoaded;
            this.ValuationService.OnValuationProgress -= ValuationService_OnValuationProgress;
            this.ValuationService.OnValuationCompleted -= ValuationService_OnValuationCompleted;
            GC.SuppressFinalize(this);
        }
    }
}
