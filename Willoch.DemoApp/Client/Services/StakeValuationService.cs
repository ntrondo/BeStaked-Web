using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesLibBeStaked.Converters;
using Willoch.DemoApp.Client.Code.Models;

namespace Willoch.DemoApp.Client.Services
{
    public interface IStakeValuationProvider
    {
        StakeValuation GetEvaluation(StakeInfo stake);
        void ClearCache();
    }
    public interface IStakeValuationService:IStakeValuationProvider
    {               
        event EventHandler OnValuationBasisChanged;
        event EventHandler OnValuationStarted;
        event EventHandler OnValuationCompleted;
        event EventHandler OnValuationProgress;
    }
    public class DailyDataLoadedEventArgs : EventArgs
    {
        public DailyDataLoadedEventArgs(int count)
        {
            this.Count = count;
        }

        public int Count { get; set; }
    }
   
    public class StakeValuationService:IStakeValuationService
    {
        protected readonly ILogger<StakeValuationService> logger;
        protected void Log(string message)
        {
            //this.logger.Log(LogLevel.Information, message);
        }
        protected IContractAsyncAccessor ContractAsyncAccessor { get => _transferableStakeAccessor; }
        private IStakeableAsyncAccessor StakeableAccessor { get => _transferableStakeAccessor; }
        private readonly ITransferableStakeAccessor _transferableStakeAccessor;
        private readonly IAssetsService _assetsAccessor;
        
        
        private CancellationTokenSource _processAssetsCTS;
        private CancellationTokenSource _loadDailyDataCTS;
        public StakeValuationService(ILogger<StakeValuationService> logger, ITransferableStakeAccessor transferableStakeAccessor, IAssetsService assetsAccessor)
        {
            this.logger = logger;
            
            this._transferableStakeAccessor = transferableStakeAccessor;
            this.StakeableAccessor.NotifyUpdate += _stakeableAccessor_NotifyUpdate;
            
            this._assetsAccessor = assetsAccessor;
            this._assetsAccessor.DataRefreshed += _assetsAccessor_DataRefreshed;
            

            this.OnDailyDataChanged += StakeValuationService_OnDailyDataChanged;
        }

        private void _assetsAccessor_DataRefreshed(object sender, EventArgs e)
        {
            this.Log("_assetsAccessor_DataRefreshed()");
            this.CancelValuation();
            this.EnsureValuating();
        }
        private void _stakeableAccessor_NotifyUpdate(object sender, EventArgs e)
        {
            //This happens on network change and account change
            //Invalidates cache and requires loading.
            this.Log("_stakeableAccessor_NotifyUpdate()");
            this.ClearCache();
            this.EnsureLoading();
        }
        
        private void StakeValuationService_OnDailyDataChanged(object sender, DailyDataLoadedEventArgs e)
        {
            this.Log("StakeValuationService_OnDailyDataChanged()");
            this.CancelValuation();
            this.ClearValuationCache();
            this.OnValuationBasisChanged?.Invoke(sender, e);
            this.EnsureValuating();
        }
        #region Cache
        protected readonly Dictionary<int, DailyData> _dailyDataByDay = new();
        private readonly Dictionary<ulong, Code.Models.StakeValuation> _valuationsByStakeId = new();
        private int _currentDay;
        private double _sharePrice;
        public virtual void ClearCache()
        {
            this.Log("ClearCache()");
            this.Cancel();
            this._dailyDataByDay.Clear();
            
            this.ClearValuationCache();
            
            this._currentDay = 0;
            this._sharePrice = 0;
        }

        private void ClearValuationCache()
        {
            this._valuationsByStakeId.Clear();
        }
        #endregion

        private void Cancel()
        {
            this.CancelValuation();
            this.CancelLoadingDailyData();                  
        }
        private void CancelLoadingDailyData()
        {
            if(_loadDailyDataCTS == null)
            {
                this.Log("CancelLoadingDailyData() was not loading");
                return;
            }
            this.Log("CancelLoadingDailyData() cancelling");
            _loadDailyDataCTS.Cancel();
            _loadDailyDataCTS.Dispose();
            _loadDailyDataCTS = null;
            
        }
       
        #region LoadData
        private bool IsLoading => this._loadDailyDataCTS != null;
        private bool IsLoaded => this._dailyDataByDay.Count >= this._currentDay - 2 && this._currentDay > 0;
        private void EnsureLoading()
        {            
            if (this.IsLoading)
            {
                //this.Log("EnsureLoading() allready loading");
                return;
            }
            if (this.IsLoaded)
            {
                //this.Log("EnsureLoading() allready loaded");
                return;
            }            
            this.Log("EnsureLoading() initiating");
            this._loadDailyDataCTS = new CancellationTokenSource();
            _ = Task.Run(async () => { await this.EnsureSharePriceIsLoaded(_loadDailyDataCTS.Token); });
            _ = Task.Run(async () => { await this.LoadDailyDataAsync(_loadDailyDataCTS.Token); });
            this.OnValuationStarted?.Invoke(this, EventArgs.Empty);
        }
        private async Task LoadDailyDataAsync(CancellationToken token)
        {
            this.Log("LoadDailyDataAsync start");

            Task loadCurrentDayTask = this.EnsureCurrentDayIsLoaded(token);
            Task loadPreloadedDailyData = this.EnsureDailyDataPreloaded(token);
            await Task.WhenAll(loadCurrentDayTask, loadPreloadedDailyData);

            //Load all mising days
            for (int i = this._currentDay - 1; i >= 0 && !token.IsCancellationRequested;)            
                i = await this.LoadDailyDataBatchAsync(i, token) - 1;
            if (!token.IsCancellationRequested)
            {
                this._loadDailyDataCTS.Dispose();
                this._loadDailyDataCTS = null;
                EnsureValuating();
            }
            this.Log("LoadDailyDataAsync end");
        }

        protected virtual Task EnsureDailyDataPreloaded(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        
        
        private async Task<int> LoadDailyDataBatchAsync(int dayToLoad, CancellationToken token)
        {
            DailyData dailyData;
            List<DailyData> loaded = new List<DailyData>();
            int target = Math.Max(0, dayToLoad - _dailyDataLoadingBatchSize);
            //this.Log("LoadDailyDataBatchAsync [" + target + ", " + dayToLoad + "]");
            for (; dayToLoad >= target; dayToLoad--)
            {
                if (this._dailyDataByDay.ContainsKey(dayToLoad))
                    continue;
                if (token.IsCancellationRequested)
                    break;
                dailyData = await this.StakeableAccessor.GetDailyDataAsync(dayToLoad, token);
                if (dailyData != null)
                {
                    loaded.Add(dailyData);
                    this._dailyDataByDay.Add(dayToLoad, dailyData);
                }
            }
            if (loaded.Any())
            {
                this.OnDailyDataChanged?.Invoke(this, new DailyDataLoadedEventArgs(loaded.Count));
                this.Log(System.Text.Json.JsonSerializer.Serialize(this._dailyDataByDay.Values.ToArray()));
            }
            return target;
        }

        protected int _dailyDataLoadingBatchSize = 10;
        private static readonly int _dailyInterestAverageCount = 10;
        private event EventHandler<DailyDataLoadedEventArgs> OnDailyDataChanged;
        protected void InvokeDailyDataChanged(object sender, DailyDataLoadedEventArgs e)
        {
            this.OnDailyDataChanged?.Invoke(sender, e);
        }
        private async Task EnsureSharePriceIsLoaded(CancellationToken token)
        {
            if (this._sharePrice > 0)
                return;
            this._sharePrice = await this.StakeableAccessor.GetSharePriceAsync(token);
        }

        private async Task EnsureCurrentDayIsLoaded(CancellationToken token)
        {
            if (this._currentDay == 0)
            {
                //this.Log("EnsureCurrentDayIsLoaded start");
                this._currentDay = await this.StakeableAccessor.GetCurrentDayAsync(token);
                this.Log("EnsureCurrentDayIsLoaded() end currentDay:" + this._currentDay);
            }
        }
        private DailyData GetLoadedDailyData(int day)
        {
            if (this._dailyDataByDay.ContainsKey(day))
                return this._dailyDataByDay[day];
            return null;
        }
        #endregion

        #region Valuation
        private bool IsValuating => this._processAssetsCTS != null;
        private void CancelValuation()
        {
            //logger.Log(LogLevel.Information, "CancelValuation()");
            _processAssetsCTS?.Cancel();
            _processAssetsCTS?.Dispose();
            _processAssetsCTS = null;
        }

        private void EnsureValuating()
        {
            //logger.Log(LogLevel.Information, "EnsureValuating()");
            this.EnsureLoading();
            if (this.IsValuating)
                return;
            this._processAssetsCTS = new CancellationTokenSource();
            _ = this.ProcessAssets(this._processAssetsCTS.Token);
        }
        private async Task ProcessAssets(CancellationToken token)
        {
            //this.logger.Log(LogLevel.Information, "ProcessAssets() start");
            await this.EnsureCurrentDayIsLoaded(token);
            await this.EnsureSharePriceIsLoaded(token);
            int i = 0, n = 5;
            foreach (var stakeType in this._assetsAccessor.WalletAssets.StakeTypes)
            {
                if (token.IsCancellationRequested)
                    break;
                foreach (var stake in this._assetsAccessor.WalletAssets[stakeType])
                {
                    if (token.IsCancellationRequested)
                        break;
                    await this.ProcessStake(stake, token);
                    i++;
                    if (i % n == 0)
                        this.OnValuationProgress?.Invoke(this, EventArgs.Empty);
                }
            }
            //if (i % n != 0 && !token.IsCancellationRequested)
            //    this.OnValuationProgress?.Invoke(this, EventArgs.Empty);
            if (token.IsCancellationRequested)
                this.logger.Log(LogLevel.Information, "ProcessAssets() cancelled");
            else
            {
                this.OnValuationCompleted?.Invoke(this, EventArgs.Empty);
                //this.logger.Log(LogLevel.Information, "ProcessAssets() end");
                this._processAssetsCTS?.Dispose();
                this._processAssetsCTS = null;
            }
        }
        public event EventHandler OnValuationBasisChanged;
        public event EventHandler OnValuationProgress;
        public event EventHandler OnValuationCompleted;
        public event EventHandler OnValuationStarted;

        private async Task ProcessStake(StakeInfo stake, CancellationToken token)
        {
            //this.logger.Log(LogLevel.Information, "ProcessStake(" + stake.StakeId + ") start");
            if (this._valuationsByStakeId.ContainsKey(stake.StakeId))
            {
                //this.Log("ProcessStake(" + stake.StakeId + ") valuation allredy exists");
                return;
            }                
            if (this._currentDay == 0)
                return;            
            double[] accruedInterestSeries = GetAccruedInterest(stake);
            double accruedInterestSum = 0, currentDailyInterest = 0;
            if (accruedInterestSeries.Any())
            {
                accruedInterestSum = accruedInterestSeries.Sum();
                currentDailyInterest = accruedInterestSeries.TakeLast(_dailyInterestAverageCount).Average();
            }          
            double bookValue = stake.StakedAmount + accruedInterestSum;
            //this.Log("ProcessStake(" + stake.StakeId + ") bv:" + bookValue);
            int lockDay = stake.LockedDay > 0 && stake.LockedDay <= this._currentDay ? stake.LockedDay : this._currentDay + 1;
            int unLockDay = stake.UnlockedDay > 0 ? stake.UnlockedDay : lockDay + stake.StakedDays;
            
            var firstDay = DateTime.UtcNow.Date.AddDays(-(this._currentDay - lockDay));
            var lastDay = DateTime.UtcNow.Date.AddDays(unLockDay - this._currentDay - 1);
            var daysRemaining = unLockDay - _currentDay;

            double marketValue = CalculateMarketValue(stake, accruedInterestSum, daysRemaining);
            double reward = await GetReward(stake, daysRemaining, bookValue, token);
            this._valuationsByStakeId.Add(stake.StakeId, new StakeValuation(accruedInterestSum, currentDailyInterest, bookValue,marketValue, firstDay, lastDay, reward));
            //this.Log("ProcessStake(" + stake.StakeId + ") end");
        }

        private async Task<double> GetReward(StakeInfo stake, int daysRemaining, double bookValue, CancellationToken token)
        {
            if (daysRemaining > 0)
                return default;            
            if(stake is TStakeInfo tStake)
            {
                var waitedDays = (ushort)(-daysRemaining);
                //this.logger.Log(LogLevel.Information, "GetReward() waitedDays:" + waitedDays);
                return await _transferableStakeAccessor.GetReward(bookValue, waitedDays, tStake.RewardStretching, token);                                
            }
            return default;
        }

        private double CalculateMarketValue(StakeInfo stake, double accruedInterests, int daysRemaining)
        {
            if (daysRemaining <= 0)
                return stake.StakedAmount + accruedInterests;
            int targetStakeLength = 5555;
            //double guess = this._sharePrice * stake.Shares / 2;

            //How much do you have to stake now for 5555 days to get the same number of shares
            double bonAdjRepCost = this.PrincipalFromSharesCalculator.Convert(new(stake.Shares, targetStakeLength, this._sharePrice));
            //double bonAdjRepCost = this.EstimatePrincipalOfNewStake(stake.Shares, targetStakeLength);
            //How much of the stake is left as a fraction of 5555

            double remAdjRepCost = bonAdjRepCost * (double)daysRemaining / targetStakeLength; //AdjustForRemaining(bonAdjRepCost, daysRemaining, targetStakeLength);
            double marketValue = remAdjRepCost + accruedInterests;
            return marketValue;
        }
        private PrincipalCalculator _principalFromSharesCalculator;
        protected PrincipalCalculator PrincipalFromSharesCalculator
        {
            get
            {
                if (this._principalFromSharesCalculator == null)
                    this._principalFromSharesCalculator = new PrincipalCalculator(1);
                return this._principalFromSharesCalculator;
            }
        }
     
        private double[] GetAccruedInterest(StakeInfo stake)
        {            
            if(stake == null || stake.LockedDay <= 0)
                return Array.Empty<double>();
            int i = stake.LockedDay <= 0 ? this._currentDay : stake.LockedDay;
            int n = stake.UnlockedDay >= stake.LockedDay ? stake.UnlockedDay : this._currentDay - 1;
            List<double> accrued = new List<double>();
            DailyData val;
            for (; i < n; i++)
            {
                val = this.GetLoadedDailyData(i);
                accrued.Add(val == null ? 0 : val.GetPayout(stake.Shares));
            }
            return accrued.ToArray();
        }

        

        public StakeValuation GetEvaluation(StakeInfo stake)
        {
            this.EnsureLoading();
            if(this._valuationsByStakeId.ContainsKey(stake.StakeId))
                return this._valuationsByStakeId[stake.StakeId];
            return null;
        }

       
        #endregion
    }
    public class StakeValuationServicePreloaded : StakeValuationService
    {
        private readonly HttpClient _httpClient;
        public StakeValuationServicePreloaded(ILogger<StakeValuationService> logger, ITransferableStakeAccessor transferableStakeAccessor, IAssetsService assetsAccessor, HttpClient httpClient) 
            : base(logger, transferableStakeAccessor, assetsAccessor)
        {
            this._httpClient = httpClient;
        }
        private bool _dailyDataPreLoadLoaded;
        public override void ClearCache()
        {
            this._dailyDataPreLoadLoaded = false;
            base.ClearCache();
        }
       
        protected override async Task EnsureDailyDataPreloaded(CancellationToken token)
        {
            if (this._dailyDataPreLoadLoaded)
                return;
            this.Log("EnsureDailyDataPreloaded loading data");            
            try
            {
                var nw = await base.ContractAsyncAccessor.GetNetworkAsync();
                //* "data/dd1.json" */
                string relativePath = string.Format("data/dd{0}.json", nw.Id);
                this.Log("Path:" + relativePath);
                string serial = await this._httpClient.GetStringAsync(relativePath, token);
                if (string.IsNullOrWhiteSpace(serial))
                    serial = "[]";
                var preLoadedData = System.Text.Json.JsonSerializer.Deserialize<DailyData[]>(serial);
                int countBefore = this._dailyDataByDay.Count;
                foreach (var dailyData in preLoadedData)
                    if (dailyData != null && !this._dailyDataByDay.ContainsKey(dailyData.Day))
                        this._dailyDataByDay.Add(dailyData.Day, dailyData);
                this._dailyDataPreLoadLoaded = true;
                int addedCount = this._dailyDataByDay.Count - countBefore;
                if (addedCount > 0)
                    this.InvokeDailyDataChanged(this, new DailyDataLoadedEventArgs(addedCount));
                this.logger.Log(LogLevel.Information, "EnsureDailyDataPreloade items:" + addedCount);
            }
            catch (TaskCanceledException)
            { }
            catch(HttpRequestException rex)
            {
                if (rex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    this.Log("EnsureDailyDataPreloaded() data file not found");
                else
                    throw;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex, "Error preloading data");
            }
        }
    }
}
