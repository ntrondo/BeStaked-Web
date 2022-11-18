using AngleSharp.Dom;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.UI.Services;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.UI
{
    public class NavMenuTests
    {
        private readonly ITestOutputHelper output;
        public NavMenuTests(ITestOutputHelper output)
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

            //Set timeout for getting elements
            var tenSeconds = new TimeSpan(0, 0, 10);
            var twoSeconds = tenSeconds / 5;

            //Render component and verify markup
            var menu = ctx.RenderComponent<NavMenu>();
            Assert.False(string.IsNullOrWhiteSpace(menu.Markup));

            //Verify that a brand is rendered
            var element = menu.Find(".navbar-item.brand");
            Assert.False(string.IsNullOrWhiteSpace(element.TextContent));

            //Wait for and verify loaded
            walletService.InvokeNotifyUpdate();
            var condition = () => { return menu.Instance.IsLoaded; };
            await Utilities.WaitForCondition(condition, twoSeconds, tenSeconds);
            Assert.True(condition());

            //Verify no provider detected
            Assert.False(menu.Instance.IsProviderDetected);
            Assert.False(menu.FindAll("div.navbar-item.provider-icon").Any());

            //Mock provider detection and verify on component
            walletService.IsProviderDetected = true;
            walletService.InvokeNotifyUpdate();
            Assert.True(menu.Instance.IsProviderDetected);

            //Verify not connected and image
            Assert.False(menu.Instance.IsConnected);
            element = menu.Find("figure");
            Assert.Contains("Not connected", element.OuterHtml);

            //Mock connection and verify on component
            //Consider rewriting the component so that onclick goes through the service and not directly to js.
            walletService.IsEnabled = true;
            walletService.InvokeNotifyUpdate();
            Assert.True(menu.Instance.IsConnected);

            //Verify provider image
            element = menu.Find("figure");
            Assert.Contains("Connected to wallet", element.OuterHtml);
        }
    }
}
