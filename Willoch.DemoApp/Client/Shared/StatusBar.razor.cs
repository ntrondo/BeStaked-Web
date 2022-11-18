using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Interfaces;
using Willoch.DemoApp.Client.Code.DispAdapt;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stake;
using Willoch.DemoApp.Shared;

namespace Willoch.DemoApp.Client.Shared
{
    public partial class StatusBar : ComponentBase, IDisposable
    {
        private bool IsLoaded { get; set; }
        public bool IsProviderDetected { get; set; }
        public bool IsConnected { get; set; }
        public NetworkInfo Network;
        public bool IsLoadingAssets { get; set; }
        public bool IsValuatingAssets { get; set; }

        
        private AssetsDisplayAdaptor Assets{ get; set; }

        [Inject]
        private IWalletConnectorService WalletService { get; set; }
        [Inject]
        private IAssetsService AssetsService { get; set; }
        [Inject]
        private IExchangeRatesService ExchangeRateService { get; set; }
        [Inject]
        private IStakeValuationService ValuationService { get; set; }
        [Inject]
        private IStorageProvider Storage { get; set; }
        [Inject]
        private ILogger<StatusBar> Logger { get; set; }


        protected override async Task OnInitializedAsync()
        {
            WalletService.NotifyUpdate += WalletService_NotifyUpdate;
            this.WalletService_NotifyUpdate(this, EventArgs.Empty);

            AssetsService.DataRefreshed += AssetsService_DataRefreshed;
            AssetsService.StatusChanged += AssetsService_StatusChanged;
            ExchangeRateService.OnExchangeRateLoaded += ExchangeRateService_OnExchangeRateLoaded;
            ValuationService.OnValuationStarted += ValuationService_OnValuationStarted;
            ValuationService.OnValuationProgress += ValuationService_OnValuationProgress;
            ValuationService.OnValuationCompleted += ValuationService_OnValuationCompleted;
            this.AssetsService_DataRefreshed(this, EventArgs.Empty);
            await base.OnInitializedAsync();            
        }
        private async void ReloadClicked()
        {
            await this.Storage.RemoveAll();
            this.ExchangeRateService.ClearCache();
            this.ValuationService.ClearCache();
            _ = AssetsService.Reload();
        }
        private void ValuationService_OnValuationStarted(object sender, EventArgs e)
        {
            //Logger.Log(LogLevel.Information, "ValuationService_OnValuationStarted()");
            this.IsValuatingAssets = true;
            InvokeAsync(this.StateHasChanged);
        }

        private void ValuationService_OnValuationCompleted(object sender, EventArgs e)
        {
            //Logger.Log(LogLevel.Information, "ValuationService_OnValuationCompleted()");
            this.IsValuatingAssets = false;
            InvokeAsync(this.StateHasChanged);
        }

        private void ValuationService_OnValuationProgress(object sender, EventArgs e)
        {
            //Logger.Log(LogLevel.Information, "ValuationService_OnValuationProgress()");
            this.IsValuatingAssets = true;
            this.RecreateAdaptor();
        }

        private void ExchangeRateService_OnExchangeRateLoaded(object sender, EventArgs e)
        {
            this.RecreateAdaptor();
        }

        private void RecreateAdaptor()
        {
            IConvert<double, ICurrencyAmount> exchangeRateModel = ExchangeRateService.GetExchangeRate();
            var stringConverter = ConstrainedDoubleAmountToShortStringConverter.Instance;
            AssetsModel assets = this.AssetsService?.WalletAssets ?? new AssetsModel();
            this.Assets = new AssetsDisplayAdaptor(assets, ValuationService);
            InvokeAsync(this.StateHasChanged);
        }
        private void AssetsService_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            bool isLoadingAssets = AssetsService.Status == AssetsRetrievalStatus.Fetching || AssetsService.Status == AssetsRetrievalStatus.Updating;
            if(this.IsLoadingAssets != isLoadingAssets)
            {
                this.IsLoadingAssets = isLoadingAssets;
                InvokeAsync(this.StateHasChanged);
            }
        }

        private void AssetsService_DataRefreshed(object sender, EventArgs e)
        {
            RecreateAdaptor();            
        }

        private async void WalletService_NotifyUpdate(object sender, EventArgs e)
        {
            this.IsProviderDetected = WalletService.IsProviderDetected;
            this.Network = await WalletService.GetNetworkAsync();
            this.IsConnected = WalletService.IsEnabled;
            if (sender != this)
            {
                this.IsLoaded = true;
                await InvokeAsync(this.StateHasChanged);
            }
        }

        public void Dispose()
        {
            WalletService.NotifyUpdate -= WalletService_NotifyUpdate;
            AssetsService.DataRefreshed -= AssetsService_DataRefreshed;
            GC.SuppressFinalize(this);
        }
    }

}
