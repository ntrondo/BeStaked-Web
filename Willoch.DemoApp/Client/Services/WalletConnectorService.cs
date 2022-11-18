using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Willoch.DemoApp.Client.Code.Models;
using Willoch.DemoApp.Shared;
using Willoch.DemoApp.Shared.Utilities;

namespace Willoch.DemoApp.Client.Services
{
    #region interfaces
    public interface IContractAsyncAccessor
    {
        Task<NetworkInfo> GetNetworkAsync();
    }
    public interface IERC20AsyncAccessor:IContractAsyncAccessor
    {
        bool IsEnabled { get; }
        bool IsProviderDetected { get; }
        event EventHandler NotifyUpdate;
        Task<double> GetBalaceAsync(CancellationToken cancellation = default);
        Task<double> GetAllowanceAsync(string spender,CancellationToken cancellation = default);
        Task<bool> Approve(string spender, double newAmount, CancellationToken cancellation = default);
    }
    public interface IStakeableAsyncAccessor : IERC20AsyncAccessor
    {
        Task<int> GetCurrentDayAsync(CancellationToken cancellation = default);
        Task<double> GetSharePriceAsync(CancellationToken cancellation = default);
        Task<DailyData> GetDailyDataAsync(int day, CancellationToken cancellation = default);
        Task<int> GetStakeCountAsync(CancellationToken cancellation = default);
        Task<StakeInfo> GetStakeInfo(int index, CancellationToken cancellation = default);
        Task CreateStake(double principal, UInt16 duration, CancellationToken cancellation = default);
        Task<bool> Mint(double amount, CancellationToken cancellation = default);
        
    }
    public interface ITransferableStakeAccessor : IStakeableAsyncAccessor
    {
        Task<int> GetTransferableStakeCountAsync(CancellationToken cancellation = default);
        Task<TStakeInfo> GetTransferrableStakeInfoForWalletAsync(int index, CancellationToken cancellation = default);
        Task<TStakeInfo> GetTransferableStakeInfoAsync(int globalIndex, CancellationToken cancellation = default);
        Task<ushort> GetRewardStretching(ulong tStakeId, CancellationToken cancellation = default);
        Task<double> GetReward(double principal, ushort waitedDays, ushort rewardStretching, CancellationToken cancellation = default);

        Task<double> GetStakeableAllowanceAsync(CancellationToken cancellation = default);
        Task<bool> ApproveStakeable(double newAmount, CancellationToken cancellation = default);
        /// <summary>
        /// Gets the fee percentage, a number between 0 and 100.
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns>Number in [0, 100]</returns>
        Task<double> GetContractFeePercentage(CancellationToken cancellation = default);

        //Task CreateTransferableStake(double principal, UInt16 duration, CancellationToken cancellation = default);
    }
    public interface IWalletConnector
    {
        [JSInvokable] Task OnDisconnect(object error);
        [JSInvokable] Task OnAccountsChanged(string[] accounts);
        [JSInvokable] Task OnChainChanged(string chainId);
        [JSInvokable] Task OnMessage(object message);
        [JSInvokable] Task EnsureAssetsLoading();
        
       
    }
    public interface IWalletConnectorService : IWalletConnector, IAsyncDisposable, ITransferableStakeAccessor  { }
    #endregion
    internal class WalletConnectorService : IWalletConnectorService
    {
        public bool IsProviderDetected { get { return this.currentNetwork != null; } }
        public bool IsConnected => IsAnyAccouns;
        public bool IsEnabled => IsAnyAccouns;
        private bool IsAnyAccouns
        {
            get
            {
                var accounts = walletState?.accounts;
                if (accounts == null || accounts.Length == 0)
                    return false;
                return true;
            }
        }
        public event EventHandler NotifyUpdate;

        
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        protected async Task WaitForModuleTask()
        {
            await moduleTask.Value;            
        }
        protected readonly ILogger<WalletConnectorService> logger;
        private readonly IJSInProcessRuntime jsRuntime;
        //private readonly IAssetsService assetsService;
        protected NetworkInfo currentNetwork;
        protected WalletState walletState;

        
        public WalletConnectorService(ILogger<WalletConnectorService> logger,IJSRuntime jsRuntime)
        {
            this.logger = logger;
            this.jsRuntime = (IJSInProcessRuntime)jsRuntime;
            moduleTask = new(InitializeAsync);
        }

        private async Task<IJSObjectReference> InitializeAsync()
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/wallet-connector.js");
            var reference = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("InitializeWalletConnector", reference);
            await module.InvokeVoidAsync("Listen");
            await this.LoadState(module);
            await this.LoadNetwork(module);
            NotifyUpdate?.Invoke(this, EventArgs.Empty);
            return module;            
        }
        private async Task LoadState(IJSObjectReference module = null, CancellationToken cancellation = default)
        {
            //this.Log2JSConsole("WalletConnectorService.LoadState()");
            if(module == null)
                module = await moduleTask.Value;
            this.walletState = await module.InvokeAsync<WalletState>("GetState", cancellation);
            //this.Log2JSConsole(this.walletState);
        }
        private async Task LoadNetwork(IJSObjectReference module = null, CancellationToken cancellation = default)
        {
            if (module == null)
                module = await moduleTask.Value;
            var id = await module.InvokeAsync<int>("GetNetworkVersion"/*"InitializeWeb3Provider"*/, cancellation);
            this.currentNetwork = id > 0 ? Constants.Networks[id] : null;
            //this.Log2JSConsole("WalletConnectorService.LoadNetwork(), network:" + (currentNetwork == null ? "null" : currentNetwork.Id.ToString()));
        }
        #region IWalletConnector
        [JSInvokable]
        public Task OnConnect(object a)
        {
            //this.Log2JSConsole("WalletConnectorService.OnConnect()");
            return Task.CompletedTask;
        }
        [JSInvokable]
        public Task OnDisconnect(object error)
        {
            this.Log2JSConsole("WalletConnectorService.OnDisconnect()");
            return Task.CompletedTask;
        }
        [JSInvokable]
        public async Task OnAccountsChanged(string[] accounts)
        {
            //this.Log2JSConsole("WalletConnectorService.OnAccountsChanged()");
            await this.LoadState();
            NotifyUpdate?.Invoke(this, EventArgs.Empty);            
        }
        [JSInvokable]
        public Task OnChainChanged(string chainId)
        {
            //this.Log2JSConsole("WalletConnectorService.OnChainChanged('" + chainId + "')");
            NotifyUpdate?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
        [JSInvokable]
        public Task OnMessage(object message)
        {
            this.Log2JSConsole("WalletConnectorService.OnMessageReceived()");
            return Task.CompletedTask;
        }
        //AssetsReader Reader = null;
        [JSInvokable]
        public Task EnsureAssetsLoading()
        {
            this.Log2JSConsole("WalletConnectorService.EnsureAssetsLoading()");
            NotifyUpdate?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
        [JSInvokable]
        public Task TestMethod()
        {
            return this.GetSharePriceAsync();
        }
        #endregion
      
        private void Log2JSConsole(object message)
        {
            jsRuntime.InvokeVoid("console.log", new object[] { message });
        }

        public async Task<NetworkInfo> GetNetworkAsync()
        {
            await moduleTask.Value;
            return currentNetwork;
        }

       
        #region IERC20AsyncReader
        public virtual async Task<double> GetBalaceAsync(CancellationToken cancellation = default)
        {
            //this.Log2JSConsole("WalletConnectorService.GetBalaceAsync()");
            double balance = 0;
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.GetBalaceAsync() no account");
                return balance;
            }
                
            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetBalaceAsync() no contract address");
                return balance;
            }
            var module = await moduleTask.Value;
            string balanceString = await module.InvokeAsync<string>("balanceOfERC20", cancellation, new object[] { address, account });
            balance = Numbers.ConvertToTokenAmount(balanceString, currentNetwork.StakeableContract);
            //this.Log2JSConsole("WalletConnectorService.GetBalaceAsync() balance=" + Math.Round(balance, 2));
            return balance;            
        }
        public virtual async Task<double> GetAllowanceAsync(string spender, CancellationToken cancellation = default)
        {
            this.Log2JSConsole("WalletConnectorService.GetAllowance()");
            double allowance = 0;
            if (string.IsNullOrEmpty(spender))
            {
                this.Log2JSConsole("WalletConnectorService.GetAllowance() no spender");
                return allowance;
            }
            var module = await moduleTask.Value;
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.GetAllowance() no account");
                return allowance;
            }

            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetAllowance() no contract address");
                return allowance;
            }
            if (spender == account)
                return Numbers.GetMaximumTokenAmount(currentNetwork.StakeableContract);
            string balanceString = await module.InvokeAsync<string>("allowanceOnERC20", cancellation, new object[] { address, account, spender });
            this.Log2JSConsole("WalletConnectorService.GetAllowance() balanceString=" + balanceString);
            allowance = Numbers.ConvertToTokenAmount(balanceString, currentNetwork.StakeableContract);
            this.Log2JSConsole("WalletConnectorService.GetAllowance() allowance=" + Math.Round(allowance, 2));
            return allowance;
        }
      

        public async Task<bool> Approve(string spender, double newAmount, CancellationToken cancellation = default)
        {
            //this.Log2JSConsole("WalletConnectorService.Approve() newAmount=" + newAmount);
            if (string.IsNullOrEmpty(spender))
            {
                this.Log2JSConsole("WalletConnectorService.Approve() no spender");
                return false;
            }
            var module = await moduleTask.Value;
            string owner = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(owner))
            {
                this.Log2JSConsole("WalletConnectorService.Approve() no owner");
                return false;
            }
            string contractAddress = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(contractAddress))
            {
                this.Log2JSConsole("WalletConnectorService.Approve() no contract address");
                return false;
            }
            if (spender == owner)
            {
                this.Log2JSConsole("WalletConnectorService.Approve() spender == owner");
                return true;
            }
            var longAmount = Numbers.ConvertFromTokenAmountToDouble(newAmount, currentNetwork?.StakeableContract);
            //this.Log2JSConsole("WalletConnectorService.Approve() longAmount=" + longAmount);
            var longAmountHexa = Numbers.ToHexaString(longAmount);
            //this.Log2JSConsole("WalletConnectorService.Approve() longAmountHexa=" + longAmountHexa);
            bool result = await module.InvokeAsync<bool>("approveOnERC20", cancellation, new object[] {contractAddress, owner, spender, longAmountHexa });
            return result;
        }
        #endregion
        #region IStakeableAsyncAccessor
        public virtual async Task<int> GetCurrentDayAsync(CancellationToken cancellation = default)
        {
            int day = 0;
            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetCurrentDayAsync() no contract address");
                return day;
            }
            if (day > 0)
                return day;
            var module = await moduleTask.Value;
            string dayString = await module.InvokeAsync<string>("currentDay", cancellation, new object[] { address });
            day = Convert.ToInt32(dayString, 16);
            return day;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="day">(0,current day)</param>
        /// <param name="cancellation"></param>
        /// <returns>[0,0] for ivalid day. Actual values for valid days.</returns>
        public virtual async Task<DailyData> GetDailyDataAsync(int day, CancellationToken cancellation = default)
        {
            DailyData data = null;
            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetDailyDataAsync() no contract address");
                return data;
            }
            var module = await moduleTask.Value;
            string hexDay = Numbers.ToHexaString(day);
            string[] hexValues = await module.InvokeAsync<string[]>("dailyData", cancellation, new object[] { address, hexDay });
            this.logger.Log(LogLevel.Information, "GetDailyDataAsync" + string.Join(", ", hexValues));
            try
            {                
                data = new DailyData(day, hexValues, this.currentNetwork.StakeableContract);
                //this.logger.Log(LogLevel.Information, "GetDailyDataAsync() data parsed");
                //this.logger.Log(LogLevel.Information, "GetDailyDataAsync() " + String.Format("d:{0}, f:{1}", data.Day, data.Factor));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetDailyDataAsync day:" + day);
                this.logger.LogError("Failed to parse hexValues:" + String.Join(", ", hexValues));
            }
            return data;
        }
        private static readonly double _shareRateFactor = 1e5;
        public virtual async Task<double> GetSharePriceAsync(CancellationToken cancellation = default)
        {
            this.Log2JSConsole("WalletConnectorService.GetSharePriceAsync()");
            double sharePrice = 0;
            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetSharePriceAsync() no contract address");
                return sharePrice;
            }
            var module = await moduleTask.Value;
            string[] hexValues = await module.InvokeAsync<string[]>("globals", cancellation, new object[] { address });            
            UInt64 shareRate = Convert.ToUInt64(hexValues[2], 16);
            double sharePriceInHearts = (shareRate / _shareRateFactor);
            sharePrice = Numbers.ConvertToTokenAmount(sharePriceInHearts, this.currentNetwork.StakeableContract);
            return sharePrice;
        }
        public async Task<int> GetStakeCountAsync(CancellationToken cancellation = default)
        {
            //this.Log2JSConsole("WalletConnectorService.GetStakeCountAsync()");
            int count = 0;
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.GetStakeCountAsync() no account");
                return count;
            }
            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetStakeCountAsync() no contract address");
                return count;
            }
            var module = await moduleTask.Value;
            string countString = await module.InvokeAsync<string>("stakeCount", cancellation, new object[] { address, account });
            count = Convert.ToInt32(countString, 16);
            return count;
        }

        public async Task<StakeInfo> GetStakeInfo(int index, CancellationToken cancellation = default)
        {
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.GetStakeInfo() no account");
                return null;
            }
            return await this.GetStakeInfo(index,account, cancellation);            
        }

        protected virtual async Task<StakeInfo> GetStakeInfo(int index, string account, CancellationToken cancellation = default)
        {
            StakeInfo info = null;
            string address = currentNetwork?.StakeableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetStakeInfo() no contract address");
                return info;
            }
            var module = await moduleTask.Value;
            string hexIndex = Numbers.ToHexaString(index);
            string[] hexValues = await module.InvokeAsync<string[]>("stakeLists", cancellation, new object[] { address, account, hexIndex });
            try
            {
                info = new StakeInfo(hexValues, this.currentNetwork.StakeableContract);
            }
            catch (Exception ex)
            {
                Log2JSConsole(ex.Message);
                Log2JSConsole(ex.StackTrace);
            }
            return info;
        }

        public async Task CreateStake(double amount, ushort days, CancellationToken cancellation = default)
        {
            var contract = currentNetwork?.StakeableContract;
            string contractAddress = contract?.Address;
            if (string.IsNullOrWhiteSpace(contractAddress))
            {
                this.Log2JSConsole("WalletConnectorService.CreateStake() no contract address");
                return;
            }
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.CreateStake() no account");
                return;
            }
            var longAmount = Numbers.ConvertFromTokenAmount(amount, contract);
            var longAmountHexa = Numbers.ToHexaString(longAmount);
            var daysHexa = Numbers.ToHexaString(days);
            var module = await moduleTask.Value;            
            await module.InvokeVoidAsync("stakeStart", cancellation, new object[] { contractAddress, account, longAmountHexa, daysHexa });
        }
        public async Task<bool> Mint(double amount, CancellationToken cancellation = default)
        {
            var contract = currentNetwork?.StakeableContract;
            string contractAddress = contract?.Address;
            if (string.IsNullOrWhiteSpace(contractAddress))
            {
                this.Log2JSConsole("WalletConnectorService.Mint() no contract address");
                return false;
            }
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.Mint() no account");
                return false;
            }
            var longAmount = Numbers.ConvertFromTokenAmount(amount, contract);
            var longAmountHexa = Numbers.ToHexaString(longAmount);
            try
            {
                var module = await moduleTask.Value;
                await module.InvokeVoidAsync("mint", cancellation, new object[] { contractAddress, account, longAmountHexa });
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion
        #region ITransferablestakeAccessor
        public virtual async Task<int> GetTransferableStakeCountAsync(CancellationToken cancellation = default)
        {
            //this.Log2JSConsole("WalletConnectorService.GetTransferableStakeCountAsync()");
            int count = 0;
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.GetTransferableStakeCountAsync() no account");
                return count;
            }

            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetTransferableStakeCountAsync() no contract address");
                return count;
            }
            var module = await moduleTask.Value;
            string countString = await module.InvokeAsync<string>("balanceOfERC721", cancellation, new object[] { address, account });
            count = Convert.ToInt32(countString, 16);
            //this.Log2JSConsole("WalletConnectorService.GetTransferableStakeCountAsync() count=" + count);
            return count;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tStakeIndex">Index of transferable stake of all wallets transferable stake</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<TStakeInfo> GetTransferrableStakeInfoForWalletAsync(int tStakeIndex, CancellationToken cancellation = default)
        {
            TStakeInfo info = null;
            string account = walletState?.accounts?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(account))
            {
                this.Log2JSConsole("WalletConnectorService.GetTransferrableStakeInfoForWalletAsync() no account");
                return info;
            }
            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetTransferrableStakeInfoForWalletAsync() no contract address");
                return info;
            }
            UInt64 tStakeId = await this.GetTransferableStakeId(cancellation, tStakeIndex, account);
            int stakeIndex = await this.GetStakeIndex(cancellation, tStakeId);
            StakeInfo sInfo = await this.GetStakeInfo(stakeIndex, address, cancellation);
            ushort rewardStretching = await this.GetRewardStretching(tStakeId, cancellation);
            info = new TStakeInfo(tStakeId, sInfo, rewardStretching);
            return info;
        }
        protected virtual async Task<ulong> GetTransferableStakeId(CancellationToken cancellation, int tStakeIndex, string account)
        {
            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetTransferableStakeId() no contract address");
                return default;
            }
            var module = await moduleTask.Value;
            string hexaTStakeIndex = Numbers.ToHexaString(tStakeIndex);
            string tStakeIdString = await module.InvokeAsync<string>("tokenOfOwnerByIndexERC721Enumerable", cancellation, new object[] { address, account, hexaTStakeIndex });
            UInt64 tStakeId = Convert.ToUInt64(tStakeIdString, 16);
            return tStakeId;
        }
        public virtual async Task<ushort> GetRewardStretching(ulong tStakeId, CancellationToken cancellation = default)
        {
            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetRewardStretching() no contract address");
                return default;
            }
            var module = await moduleTask.Value;
            string tStakeIdString = Numbers.ToHexaString(tStakeId);
            var hexValues = await module.InvokeAsync<string[]>("idToToken", new object[] { address, tStakeIdString });
            ushort rs = Convert.ToUInt16(hexValues[3], 16);
            return rs;
        }
        public virtual async Task<double> GetReward(double principal, ushort waitedDays, ushort rewardStretching, CancellationToken cancellation = default)
        {
            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetReward() no contract address");
                return default;
            }
            var module = await moduleTask.Value;
            var arguments = new object[]
            {
                address,
                Numbers.ToHexaString(Numbers.ConvertFromTokenAmount(principal, this.currentNetwork.StakeableContract)),
                Numbers.ToHexaString(waitedDays),
                Numbers.ToHexaString(rewardStretching)
            };
            string rewardString = await module.InvokeAsync<string>("calculateReward", cancellation, arguments);
            double rewardAmount = Numbers.ConvertToTokenAmount(rewardString, this.currentNetwork.StakeableContract);
            return rewardAmount;
        }
        protected virtual async Task<int> GetStakeIndex(CancellationToken cancellation, ulong tStakeId)
        {
            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetStakeIndex() no contract address");
                return default;
            }
            var module = await moduleTask.Value;
            var tStakeIdString = Numbers.ToHexaString(tStakeId);
            string hexaStakeIndex = await module.InvokeAsync<string>("getStakeIndex", cancellation, new object[] { address, tStakeIdString });
            int stakeIndex = Convert.ToInt32(hexaStakeIndex, 16);
            return stakeIndex;
        }
        public async Task<TStakeInfo> GetTransferableStakeInfoAsync(int globalIndex, CancellationToken cancellation = default)
        {
            //this.Log2JSConsole("WalletConnectorService.GetTransferableStakeInfo(" + globalIndex + ")");
            TStakeInfo info = null;
            string address = currentNetwork?.TransferableContract.Address;
            if (string.IsNullOrWhiteSpace(address))
            {
                this.Log2JSConsole("WalletConnectorService.GetTransferableStakeInfo(globalIndex) no contract address");
                return info;
            }
            var module = await moduleTask.Value;
            string hexaIndex = Numbers.ToHexaString(globalIndex);
            string tStakeIdString = await module.InvokeAsync<string>("tokenByIndexERC721Enumerable", cancellation, new object[] { address, hexaIndex });
            UInt64 tStakeId = Convert.ToUInt64(tStakeIdString, 16);
            hexaIndex = await module.InvokeAsync<string>("getStakeIndex", cancellation, new object[] { address,tStakeIdString });
            globalIndex = Convert.ToInt32(hexaIndex, 16);
            var sinfo = await GetStakeInfo(globalIndex, address);
            ushort rewardStretching = await this.GetRewardStretching(tStakeId, cancellation);
            info = new TStakeInfo(tStakeId, sinfo, rewardStretching);
            this.Log2JSConsole(info.Summary);
            return info;
        }
        public async Task<double> GetStakeableAllowanceAsync(CancellationToken cancellation = default)
        {
            await this.WaitForModuleTask();
            string spender = currentNetwork?.TransferableContract.Address;
            return await this.GetAllowanceAsync(spender, cancellation);
        }
        public async Task<bool> ApproveStakeable(double newAmount, CancellationToken cancellation = default)
        {
            await this.WaitForModuleTask();
            string spender = this.currentNetwork?.TransferableContract?.Address;
            return await this.Approve(spender, newAmount, cancellation);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns>A number in the interval [0, 100]</returns>
        public async Task<double> GetContractFeePercentage(CancellationToken cancellation = default)
        {
            this.Log2JSConsole("WalletConnectorService.GetContractFeePercentage()");
            var module = await moduleTask.Value;
            string hexStr = await module.InvokeAsync<string>("getOwnerFeePermille", new object[] { currentNetwork.TransferableContract.Address });
            UInt16 permille = Convert.ToUInt16(hexStr, 16);
            this.Log2JSConsole("WalletConnectorService.GetContractFeePercentage() permille=" + permille);
            double percentage = permille / 10.0;
            this.Log2JSConsole("WalletConnectorService.GetContractFeePercentage() percentage=" + percentage);
            return percentage;
        }
        #endregion
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
            GC.SuppressFinalize(this);
        }

       
    }
    internal class WalletConnectorServiceCached : WalletConnectorService
    {
        protected readonly IStorageProvider storage;
        public WalletConnectorServiceCached(ILogger<WalletConnectorService> logger, IJSRuntime jsRuntime, IStorageProvider storage)
            : base(logger, jsRuntime)
        {
            this.storage = storage;
        }
        #region IERC20AsyncReader
        public override async Task<double> GetBalaceAsync(CancellationToken cancellation = default)
        {
            double balance = 0;
            string key = null;
            string account = walletState?.accounts?.FirstOrDefault();
            if(account != null)
            {
                key = string.Format("{0} {1} {2}", currentNetwork.Type, account.Substring(2, 5), "HEX");
                balance = await storage.Retrieve<double>(key);
            }
            if(balance == 0)
            {
                balance = await base.GetBalaceAsync(cancellation);
                await storage.Store(key, balance, StorageExpirationPolicy.FiveMinutes);
            }
            return balance;
        }
        #endregion
        #region IStakeableAsyncAccessor
        public override async Task<DailyData> GetDailyDataAsync(int day, CancellationToken cancellation = default)
        {
            //this.logger.Log(LogLevel.Information, "Cached.GetDailyDataAsync()");
            string key = string.Format("{0} {1} {2}", currentNetwork.Type, "dailyData", day);
            DailyData data = await this.storage.Retrieve<DailyData>(key);
            if (data == null) 
            {
                data = await base.GetDailyDataAsync(day, cancellation);
                await storage.Store(key, data, StorageExpirationPolicy.Infinite);
            }
            return data;
        }
        public override async Task<int> GetCurrentDayAsync(CancellationToken cancellation = default)
        {
            string key = String.Format("{0} {1}", this.currentNetwork?.Type, "currentDay");
            int day = await storage.Retrieve<int>(key);
            if(day > 0) return day;
            day = await base.GetCurrentDayAsync(cancellation);
            if (day > 0) await storage.Store(key, day, StorageExpirationPolicy.UntilMidnightUTC);
            return day;
        }
        public override async Task<double> GetSharePriceAsync(CancellationToken cancellation = default)
        {
            double sharePrice = 0;
            await base.WaitForModuleTask();
            string nw = this.currentNetwork?.Type;
            if(string.IsNullOrEmpty(nw))
            {
                this.logger.Log(LogLevel.Information, "WalletConnectorServiceCached.GetSharePriceAsync() no network");
                return sharePrice;
            }
            string key = String.Format("{0} {1}", this.currentNetwork?.Type, "sharePrice");
            this.logger.Log(LogLevel.Information, "WalletConnectorServiceCached.GetSharePriceAsync() key=" + key);
            sharePrice = await storage.Retrieve<double>(key);
            if (sharePrice <= 0 && !cancellation.IsCancellationRequested)
            {
                //this.logger.Log(LogLevel.Information, "WalletConnectorServiceCached.GetSharePriceAsync() loading from chain");
                sharePrice = await base.GetSharePriceAsync(cancellation);    
                if(sharePrice > 0 || !cancellation.IsCancellationRequested)
                {
                    Task store = storage.Store(key, sharePrice, StorageExpirationPolicy.OneHour);
                    if (!cancellation.IsCancellationRequested)
                        await store;
                }
            }
            return sharePrice;
        }
        protected override async Task<StakeInfo> GetStakeInfo(int index, string account, CancellationToken cancellation = default)
        {
            string key = string.Format("{0} {1} {2} {3}", this.currentNetwork?.Type, account.Substring(2, 5), index, "stake");
            StakeInfo info = await storage.Retrieve<StakeInfo>(key);
            if(info == null)
            {
                info = await base.GetStakeInfo(index, account, cancellation);
                await storage.Store(key, info, StorageExpirationPolicy.OneHour);
            }
            return info;
        }
        #endregion
        #region ITransferablestakeAccessor
        public override async Task<ushort> GetRewardStretching(ulong tStakeId, CancellationToken cancellation = default)
        {
            string key = string.Format("{0} {1} {2}", this.currentNetwork?.Type, "rewStr", tStakeId);
            var rs = await storage.Retrieve<ushort>(key);
            if(rs == 0)
            {
                rs = await base.GetRewardStretching(tStakeId, cancellation);
                await storage.Store(key, rs, StorageExpirationPolicy.Infinite);
            }
            return rs;
        }
        public override async Task<double> GetReward(double principal, ushort waitedDays, ushort rewardStretching, CancellationToken cancellation = default)
        {
            string key = string.Format("{0} p{1} d{2} s{3}", "rew", Math.Round(principal), waitedDays, rewardStretching);
            bool isStored = await storage.ContainsKey(key);
            if (isStored)
                return await storage.Retrieve<double>(key);
            var rew = await base.GetReward(principal,waitedDays, rewardStretching, cancellation);
            if(!cancellation.IsCancellationRequested)
                await storage.Store(key, rew, StorageExpirationPolicy.Infinite);
            return rew;
        }
        #endregion

    }
    internal class WalletConnectorServiceExcessivelyCached : WalletConnectorServiceCached
    {
        public WalletConnectorServiceExcessivelyCached(ILogger<WalletConnectorService> logger, IJSRuntime jsRuntime, IStorageProvider storage) : base(logger, jsRuntime, storage)
        { }
        #region ITransferablestakeAccessor
        public override async Task<int> GetTransferableStakeCountAsync(CancellationToken cancellation = default)
        {
            string account = walletState?.accounts?.FirstOrDefault();
            string key = string.Format("{0} {1} {2} {3}", this.currentNetwork?.Type, account?.Substring(2, 5), "transferable", "count");
            int count = await storage.Retrieve<int>(key);
            if (count == 0)
            {
                count = await base.GetTransferableStakeCountAsync(cancellation);
                await storage.Store(key, count, StorageExpirationPolicy.OneHour);
            }
            return count;
        }

        
        protected override async Task<ulong> GetTransferableStakeId(CancellationToken cancellation, int tStakeIndex, string account)
        {
            string key = string.Format("{0} {1} {2} {3}", this.currentNetwork?.Type, account?.Substring(2, 5), "index", tStakeIndex);
            ulong id = await storage.Retrieve<ulong>(key);
            if (id == 0)
            {
                id = await base.GetTransferableStakeId(cancellation, tStakeIndex, account);
                await storage.Store(key, id, StorageExpirationPolicy.OneHour);
            }
            return id;
        }
        protected override async Task<int> GetStakeIndex(CancellationToken cancellation, ulong tStakeId)
        {
            string account = walletState?.accounts?.FirstOrDefault();
            string key = string.Format("{0} {1} {2} {3}", this.currentNetwork?.Type, account?.Substring(2, 5), "index", tStakeId);
            int index = await storage.Retrieve<int>(key);
            if (index == 0)
            {
                index = await base.GetStakeIndex(cancellation, tStakeId);
                await storage.Store(key, index, StorageExpirationPolicy.OneHour);
            }
            return index;
        }
        #endregion
    }
}
