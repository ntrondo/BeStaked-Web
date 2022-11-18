using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

using Willoch.DemoApp.Client.Services;

namespace Willoch.DemoApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services
                .AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })  
                .AddSingleton<IStorageProvider, LocalStorageService>()
                .AddSingleton<IWalletConnectorService, WalletConnectorServiceCached>()
                .AddSingleton<IWalletConnector>(provider => provider.GetService<IWalletConnectorService>())
                .AddSingleton<ITransferableStakeAccessor>(provider => provider.GetService<IWalletConnectorService>())
                .AddSingleton<IStakeableAsyncAccessor>(provider => provider.GetService<IWalletConnectorService>())
                .AddSingleton<IExchangeRatesService, ExchangeRateServiceStorageCached>()
                .AddSingleton<IAssetsService, AssetsService>()
                .AddSingleton<IStakeValuationService, StakeValuationServicePreloaded>();

            await builder.Build().RunAsync();
        }
    }
}
