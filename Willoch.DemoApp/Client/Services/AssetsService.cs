using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Client.Shared.Stakes;
using Willoch.DemoApp.Shared;
namespace Willoch.DemoApp.Client.Services
{
    public enum AssetsRetrievalStatus { NotStarted, Fetching, Updating, Idle }
    public sealed class StatusChangedEventArgs : EventArgs
    {
        public AssetsRetrievalStatus Status { get; }

        public StatusChangedEventArgs(AssetsRetrievalStatus status)
            => Status = status;

    }
    public interface IAssetsService
    {
        Task Reload();
        void AssumeTransfer(double stakeableAmount);
        AssetsModel WalletAssets { get; }
        AssetsRetrievalStatus Status { get; }

        event EventHandler DataRefreshed;
        event EventHandler<StatusChangedEventArgs> StatusChanged;
    }
  
    internal class AssetsService : IAssetsService
    {
        public event EventHandler DataRefreshed;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        private readonly ILogger logger;
        private readonly ITransferableStakeAccessor accessor;

        //private IExchangeRatesService exchangeRateService;

        private CancellationTokenSource cts;

        public AssetsService(ILogger<AssetsService> logger, ITransferableStakeAccessor accessor/*, IExchangeRatesService exchangeRateService*/)
        {
            this.logger = logger;
            this.accessor = accessor;
            //this.exchangeRateService = exchangeRateService;
            this.accessor.NotifyUpdate += (s, e) => StartUpdate();
        }
        public void AssumeTransfer(double stakeableAmount)
        {
            if (stakeableAmount == 0 || WalletAssets == null)
                return;
            double balance = stakeableAmount + WalletAssets.StakeableBalance;            
            var ls = WalletAssets[StakeType.Legacy];
            var ts = WalletAssets[StakeType.Transferable];
            this.WalletAssets = new AssetsModel(balance, ls, ts);
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }
        public AssetsRetrievalStatus Status { get; private set; }
        public AssetsModel WalletAssets { get; private set; }
        public Task Reload()
        {            
            this.StartUpdate();
            return Task.CompletedTask;
        }
        public void StartUpdate()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            UpdateAssetsAsync(cts.Token);
        }
        private LoadingAssetsModel walletAssetsLoadingTasks;
        private void UpdateAssetsAsync(CancellationToken cancellation = default)
        {
            this.walletAssetsLoadingTasks = new LoadingAssetsModel(                
                accessor.GetBalaceAsync(cancellation),
                accessor.GetStakeCountAsync(cancellation),
                accessor.GetTransferableStakeCountAsync(cancellation), 
                cancellation);
            this.walletAssetsLoadingTasks.TaskCompleted += WalletAssetsLoadingTaskCompleted; 

            Status = WalletAssets == null ? AssetsRetrievalStatus.Fetching : AssetsRetrievalStatus.Updating;
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(Status));
        }

        private void WalletAssetsLoadingTaskCompleted(object sender, AssetsLoadingTaskCompletedEventArgs e)
        {
            
            var walt = this.walletAssetsLoadingTasks;
            if(e.Task == walt.LoadLegacyStakeCountTask && walt.LoadLegacyStakeTasks == null)
            {
                walt.LoadLegacyStakeTasks = Enumerable.Range(0, walt.LoadLegacyStakeCountTask.Result).Select(i => accessor.GetStakeInfo(i, walt.Cancellation)).ToArray();
            }                
            else if(e.Task == walt.LoadTransferableStakeCountTask && walt.LoadTransferableStakeTasks == null)
                walt.LoadTransferableStakeTasks = Enumerable.Range(0,walt.LoadTransferableStakeCountTask.Result).Select(i => accessor.GetTransferrableStakeInfoForWalletAsync(i, walt.Cancellation)).ToArray();
            else
            {
                this.WalletAssets = walt.GetAssets();
                this.DataRefreshed?.Invoke(this, EventArgs.Empty);
            }  
            if(walt.IsCompleted)
            {
                //logger.Log(LogLevel.Information, "WalletAssetsLoadingTaskCompleted() WalletAssetsLoadingTasks is completed");
                Status = AssetsRetrievalStatus.Idle;
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(Status));
                walt.TaskCompleted -= WalletAssetsLoadingTaskCompleted;
            }
        }

       
    }   
}
