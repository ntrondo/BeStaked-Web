using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Bunit;
using Willoch.DemoApp.Client.Shared;
using Willoch.DemoApp.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.UI.Services;
using Xunit.Abstractions;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace UnitTests.UI
{
    public class StaturBarTests
    {
        private readonly ITestOutputHelper output;
        public StaturBarTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async void Test1()
        {
            //Create context
            using var ctx = new TestContext();

            //Add services to context
            {
                IWalletConnectorService ws = new MockWalletConnectorService();
                IAssetsService ass = new MockAssetsService(new MockLogger<MockAssetsService>(output), ws);
                ctx.Services.AddSingleton<IStorageProvider>(new MockStorageProvider());
                ctx.Services.AddSingleton(ws);
                ctx.Services.AddSingleton<IExchangeRatesService>(new MockExchangeRatesService());
                ctx.Services.AddSingleton(ass);
                ctx.Services.AddSingleton<IStakeValuationService>(new MockStakeValuationService(new MockLogger<StakeValuationService>(output), ws, ass));
            }

            //Get service for manipulation in test
            var walletService = (MockWalletConnectorService)ctx.Services.GetRequiredService<IWalletConnectorService>();
            var assetsService = ctx.Services.GetRequiredService<IAssetsService>();
            var valuationService = ctx.Services.GetRequiredService<IStakeValuationService>();
            var exchangeService = (MockExchangeRatesService)ctx.Services.GetRequiredService<IExchangeRatesService>();

            //Set timeout for getting elements
            var tenSeconds = new TimeSpan(0, 0, 10);
            var twoSeconds = tenSeconds / 5;

            //Render component and verify markup
            var bar = ctx.RenderComponent<StatusBar>();
            Assert.False(string.IsNullOrWhiteSpace(bar.Markup));

            //Verify no provider message
            Assert.False(bar.Instance.IsProviderDetected);
            var element = bar.WaitForElement(".no-provider-section", tenSeconds);
            Assert.NotNull(element);
            Assert.Contains("No ethereum wallet was detected", element.TextContent);

            //Mock provider detection and verify on component
            walletService.IsProviderDetected = true;
            walletService.InvokeNotifyUpdate();
            Assert.True(bar.Instance.IsProviderDetected);

            //Verify no connection message
            Assert.False(bar.Instance.IsConnected);
            element = bar.WaitForElement("div.not-connected", tenSeconds);
            Assert.Contains("Not connected to ethereum wallet", element.TextContent);

            //Mock connection and verify on component
            walletService.IsEnabled = true;
            walletService.InvokeNotifyUpdate();
            Assert.True(bar.Instance.IsConnected);

            //Wait for IsLoadingAssets to be false and verify
            await Utilities.WaitForCondition(() => !bar.Instance.IsLoadingAssets, twoSeconds, tenSeconds);
            Assert.False(bar.Instance.IsLoadingAssets);

            //Verify token symbol display
            element = bar.Find("td.stakeable.symbol");
            Assert.NotNull(element);
            Assert.Equal("HEX", element.InnerHtml);

            //Verify balance display
            var amount = AmountModelFactory.Create(assetsService.WalletAssets.StakeableBalance);
            element = bar.Find("td.balance");
            Assert.NotNull(element);
            Assert.Equal(amount.Scaled.AmountString, element.InnerHtml);

            //Wait for valuation to finish
            await Utilities.WaitForCondition(() => { return !bar.Instance.IsValuatingAssets; }, twoSeconds, tenSeconds);
            Assert.False(bar.Instance.IsValuatingAssets, "Did not stop valuating assets after " + tenSeconds.TotalSeconds + " seconds");

            //Verify legacy staked
            element = bar.Find("td.balance.legacy.stakeable");
            Assert.NotNull(element);
            amount = AmountModelFactory.Create(assetsService.WalletAssets[StakeType.Legacy].Select(s => valuationService.GetEvaluation(s)).Sum(v => v.BookValue));
            Assert.True(amount.Amount > 0);
            output.WriteLine("Legacy staked: " + element.TextContent);
            Assert.Equal(amount.Scaled.AmountString, element.TextContent);

            //Verify transferrable staked
            element = bar.Find("td.balance.wrapped.stakeable");
            Assert.NotNull(element);
            amount = AmountModelFactory.Create(assetsService.WalletAssets[StakeType.Transferable].Select(s => valuationService.GetEvaluation(s)).Sum(v => v.MarketValue));
            Assert.True(amount.Amount > 0);
            output.WriteLine("Transferable staked: " + element.TextContent);
            Assert.Equal(amount.Scaled.AmountString, element.TextContent);

            //Verify stakeable sum
            double sum = assetsService.WalletAssets.StakeableBalance
                + assetsService.WalletAssets[StakeType.Legacy].Select(s => valuationService.GetEvaluation(s)).Sum(v => v.BookValue)
                + assetsService.WalletAssets[StakeType.Transferable].Select(s => valuationService.GetEvaluation(s)).Sum(v => v.MarketValue);
            amount = AmountModelFactory.Create(sum);
            element = bar.Find("td.balance.total.stakeable");
            output.WriteLine("Sum: " + element.TextContent);
            Assert.Equal(amount.Scaled.AmountString, element.TextContent);

            //Verify zero fiat sum
            Assert.Equal(0, amount.Converted.Amount);
            element = bar.Find("td.balance.total.fiat");
            output.WriteLine("Sum: " + element.TextContent);
            Assert.Equal(amount.Converted.Scaled.AmountString, element.TextContent);

            //Release exchange rate and verify non-zero conversion
            exchangeService.ReturnFlag = true;
            var condition = () => { return AmountModelFactory.Create(1).Converted.Amount > 0; };
            await Utilities.WaitForCondition(condition, twoSeconds, tenSeconds);
            Assert.True(condition());

            //Verify fiat sum
            amount = AmountModelFactory.Create(sum);
            if(sum > 0)
                Assert.True(amount.Converted.Amount > 0);
            element = bar.Find("td.balance.total.fiat");
            output.WriteLine("Sum: " + element.TextContent);
            Assert.Equal(amount.Converted.Scaled.AmountString, element.TextContent);

            //Verify fiat symbol
            element = bar.Find("td.symbol.fiat");
            Assert.Equal(amount.Converted.Original.Currency.SymbolOrTicker, element.TextContent);
        }

        
    }
}
