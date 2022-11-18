using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesLibBeStaked.Converters;
using Willoch.DemoApp.Client.Code.Calculate;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;

namespace UnitTests.UI.Services
{
    internal class MockAssetsService:IAssetsService
    {
        private readonly ITransferableStakeAccessor accessor;
        private readonly ILogger logger;
        public AssetsModel? WalletAssets { get; private set; }

        public AssetsRetrievalStatus Status { get; set; } = AssetsRetrievalStatus.NotStarted;


        public event EventHandler? DataRefreshed;
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        public MockAssetsService(ILogger<MockAssetsService> logger, ITransferableStakeAccessor accessor)
        {
            this.accessor = accessor;
            this.logger = logger;
            this.accessor.NotifyUpdate += Accessor_NotifyUpdate;
        }

        private void Accessor_NotifyUpdate(object? sender, EventArgs e)
        {
            if (this.accessor.IsEnabled)
                _ = Task.Run(async () => { await GenerateAssets(); });
            else
                _ = Task.Run(async () => { await ClearAssets(); });
        }

        private Task ClearAssets()
        {
            this.WalletAssets = new AssetsModel();
            this.Status = AssetsRetrievalStatus.Idle;
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(Status));
            this.DataRefreshed?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
      
        public SharesCalculator? SharesCalculator { get; private set; }
        ulong StakeId = 800000;
        ulong TStakeId = 900000;
        private async Task GenerateAssets()
        {
            this.Status = AssetsRetrievalStatus.Fetching;
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(Status));

            double sp = await accessor.GetSharePriceAsync();
            this.SharesCalculator = new SharesCalculator(sp);

            
            var legacyStakes = new List<StakeInfo>();
            legacyStakes.Add(CreateStake(8.5e5, 100, 1000, false));

            var tStakes = new List<TStakeInfo>();
            tStakes.Add(CreateStake(94.54e7, 100, 5555));

            this.WalletAssets = new AssetsModel(183.33e3, legacyStakes.ToArray(), tStakes.ToArray());

            this.Status = AssetsRetrievalStatus.Idle;
            this.DataRefreshed?.Invoke(this, EventArgs.Empty);
            this.StatusChanged?.Invoke(this, new StatusChangedEventArgs(Status));            
        }

        private TStakeInfo CreateStake(double principal, ushort startDay, ushort duration)
        {
            var scr = SharesCalculator == null ? null : SharesCalculator.Convert(new(principal, duration));
            var shares = scr == null ? 0 : (ulong)scr.Shares;
            return new TStakeInfo(TStakeId++,new StakeInfo(StakeId++, principal, shares, startDay, duration, 0, false), 60);
        }

        private StakeInfo CreateStake(double principal, ushort startDay, ushort duration, bool autoStake = false)
        {
            var scr = SharesCalculator == null ? null : SharesCalculator.Convert(new(principal, duration));
            var shares = scr == null ? 0 : (ulong)scr.Shares;
            return new StakeInfo(StakeId++, principal, shares, startDay, duration, 0, autoStake);
        }

        public Task Reload()
        {
            throw new NotImplementedException();
        }
    }
}
