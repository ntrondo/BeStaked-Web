using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Willoch.DemoApp.Client.Code.Models
{
    public class LoadingAssetsModel
    {
        public bool IsCompleted => _tasks.All(t => t.IsCompleted);
        private readonly List<Task> _tasks = new();
        public EventHandler<AssetsLoadingTaskCompletedEventArgs> TaskCompleted;

        public CancellationToken Cancellation { get; }
        public Task<double> LoadBalanceTask { get; }
        public Task<int> LoadLegacyStakeCountTask { get; }
        private Task<StakeInfo>[] _loadLegacyStakeTasks;
        public Task<StakeInfo>[] LoadLegacyStakeTasks 
        {
            get => this._loadLegacyStakeTasks;
            set
            {
                this._loadLegacyStakeTasks = value;
                _ = this.NotifyWhenTaskIsCompleted(value);
            }
        }

      

        public Task<int> LoadTransferableStakeCountTask { get;}
        private Task<TStakeInfo>[] _loadTransferableStakeTasks;
        public Task<TStakeInfo>[] LoadTransferableStakeTasks
        {
            get => this._loadTransferableStakeTasks;
            set
            {
                this._loadTransferableStakeTasks = value;
                _ = this.NotifyWhenTaskIsCompleted(value);
            }
        }

        public LoadingAssetsModel(             
            Task<double> loadBalanceTask, 
            Task<int> loadLegacyStakeCountTask, 
            Task<int> loadTransferableStakeCountTask,
            CancellationToken cancellation)
        {
            this.Cancellation = cancellation;
            this.LoadBalanceTask = loadBalanceTask;
            this.LoadLegacyStakeCountTask = loadLegacyStakeCountTask;
            this.LoadTransferableStakeCountTask = loadTransferableStakeCountTask;

            _ = NotifyWhenTaskIsCompleted(this.LoadBalanceTask);
            _ = NotifyWhenTaskIsCompleted(this.LoadLegacyStakeCountTask);
            _ = NotifyWhenTaskIsCompleted(this.LoadTransferableStakeCountTask);
        }
        private async Task NotifyWhenTaskIsCompleted(Task[] tasks)
        {
            if (tasks == null)
                return;
            var waitForTasks = tasks.Select(t => NotifyWhenTaskIsCompleted(t)).ToArray();
            await Task.WhenAll(waitForTasks);
        }
        private async Task NotifyWhenTaskIsCompleted(Task task)
        {
            _tasks.Add(task);
            await task;
            this.TaskCompleted?.Invoke(this, new AssetsLoadingTaskCompletedEventArgs(task));
        }

        internal AssetsModel GetAssets()
        {
            double b = this.LoadBalanceTask.IsCompleted ? this.LoadBalanceTask.Result : 0;
            var ls = this.LoadLegacyStakeTasks == null ? Array.Empty<StakeInfo>() 
                : this.LoadLegacyStakeTasks.Where(t => t.IsCompletedSuccessfully).Select(t => t.Result).ToArray();
            var ts = this.LoadTransferableStakeTasks == null ? Array.Empty<StakeInfo>()
                : this.LoadTransferableStakeTasks.Where(t => t.IsCompletedSuccessfully).Select(t => t.Result).ToArray();
            return new AssetsModel(b, ls, ts);
        }
    }
    public class AssetsLoadingTaskCompletedEventArgs : EventArgs
    {
        public AssetsLoadingTaskCompletedEventArgs(Task task)
        {
            this.Task = task;
        }

        public Task Task { get; }
    }
    public class AssetsModel
    {
        private readonly Dictionary<Shared.Stakes.StakeType, StakeInfo[]> _stakesByType = new();
        public IEnumerable<Shared.Stakes.StakeType> StakeTypes => this._stakesByType.Keys;
        public StakeInfo[] this[Shared.Stakes.StakeType type]
        {
            get => _stakesByType.ContainsKey(type) ? _stakesByType[type] : Array.Empty<StakeInfo>();
        }
        public AssetsModel() { }
        public AssetsModel(double stakeableBalance, StakeInfo[] legacyStakes, StakeInfo[] transferableStakes)
        {
            this.StakeableBalance = stakeableBalance;
            this._stakesByType.Add(Shared.Stakes.StakeType.Legacy, legacyStakes);
            this._stakesByType.Add(Shared.Stakes.StakeType.Transferable, transferableStakes);
        }
        public double StakeableBalance { get; }
    }
}
