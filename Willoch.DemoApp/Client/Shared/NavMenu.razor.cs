using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client.Shared
{
    public partial class NavMenu : ComponentBase
    {
        [Inject]
        private IWalletConnectorService WalletService { get; set; }
        public bool IsLoaded { get; private set; }
        public bool IsProviderDetected { get; private set; }
        public bool IsConnected { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            WalletService.NotifyUpdate += WalletService_NotifyUpdate;
            this.WalletService_NotifyUpdate(this, EventArgs.Empty);
            await base.OnInitializedAsync();
        }

        private void WalletService_NotifyUpdate(object sender, EventArgs e)
        {
            this.IsProviderDetected = WalletService.IsProviderDetected;
            this.IsConnected = WalletService.IsEnabled;
            if(sender != this)
            {
                this.IsLoaded = true;
                InvokeAsync(this.StateHasChanged);
            }                
        }
       
    }
}
