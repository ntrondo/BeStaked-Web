using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Willoch.DemoApp.Client;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Shared;
using Willoch.DemoApp.Shared.Utilities;

namespace UnitTests.UI.Services
{
    internal class MockWalletConnectorService : IWalletConnectorService
    {
      
        public bool IsEnabled { get; set; }

        public bool IsProviderDetected { get; set; }
        internal MockWalletConnectorService() { }

        public event EventHandler? NotifyUpdate;
        public void InvokeNotifyUpdate()
        {
            NotifyUpdate?.Invoke(this, EventArgs.Empty);
        }

        public Task<bool> Approve(string spender, double newAmount, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApproveStakeable(double newAmount, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task CreateStake(double principal, ushort duration, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task EnsureAssetsLoading()
        {
            InvokeNotifyUpdate();
            return Task.CompletedTask;
        }

        public Task<double> GetAllowanceAsync(string spender, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<double> GetBalaceAsync(CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<double> GetContractFeePercentage(CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCurrentDayAsync(CancellationToken cancellation = default)
        {
            return Task.FromResult(1000);
        }

        public Task<DailyData> GetDailyDataAsync(int day, CancellationToken cancellation = default)
        {
            return Task.FromResult(new DailyData() { Day = day, Factor = 7e-12 });
        }

        public Task<NetworkInfo?> GetNetworkAsync()
        {
            if (this.IsProviderDetected)
                return Task.FromResult<NetworkInfo?>(Constants.Networks[1]);
            return Task.FromResult<NetworkInfo?>(null);
        }

        public Task<double> GetReward(double principal, ushort waitedDays, ushort rewardStretching, CancellationToken cancellation = default)
        {
            if (waitedDays >= rewardStretching)
                return Task.FromResult(principal);
            if (waitedDays <= 0)
                return Task.FromResult((double)0);
            return Task.FromResult(principal * Math.Pow(0.5, rewardStretching - waitedDays));
        }

        public Task<ushort> GetRewardStretching(ulong tStakeId, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<double> GetSharePriceAsync(CancellationToken cancellation = default)
        {            
            return Task.FromResult(0.00000003);
        }

        public Task<double> GetStakeableAllowanceAsync(CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetStakeCountAsync(CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<StakeInfo> GetStakeInfo(int index, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTransferableStakeCountAsync(CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<TStakeInfo> GetTransferableStakeInfoAsync(int globalIndex, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task<TStakeInfo> GetTransferrableStakeInfoForWalletAsync(int index, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }

        public Task OnAccountsChanged(string[] accounts)
        {
            throw new NotImplementedException();
        }

        public Task OnChainChanged(string chainId)
        {
            throw new NotImplementedException();
        }

        public Task OnDisconnect(object error)
        {
            throw new NotImplementedException();
        }

        public Task OnMessage(object message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Mint(double amount, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}
