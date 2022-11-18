using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesLibBeStaked.Converters;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Convert;

namespace Willoch.DemoApp.Client.Services
{
    public interface IExchangeRatesService
    {
        event EventHandler OnExchangeRateLoaded;
        Task<HEXToFiatCurrencyConverter> GetExchangeRateAsync(string fromTicker, string toTicker);
        HEXToFiatCurrencyConverter GetExchangeRate(string fromTicker, string toTicker);
        HEXToFiatCurrencyConverter GetExchangeRate();
        void ClearCache();
    }
    public abstract class BaseExchangeRateService : IExchangeRatesService
    {
        public event EventHandler OnExchangeRateLoaded;
        protected readonly Dictionary<string, Task<HEXToFiatCurrencyConverter>> _exchangeRatesServiceLoadingTasks = new();
        public Task<HEXToFiatCurrencyConverter> GetExchangeRateAsync(string fromTicker, string toTicker) => GetLoadingTask(fromTicker, toTicker);
        protected Task<HEXToFiatCurrencyConverter> GetLoadingTask(string fromTicker, string toTicker)
        {
            string key = fromTicker + toTicker;
            if (this._exchangeRatesServiceLoadingTasks.ContainsKey(key))
                return this._exchangeRatesServiceLoadingTasks[key];
            Task<HEXToFiatCurrencyConverter> task = CreateLoadingTaskVirtual(fromTicker, toTicker);
            this._exchangeRatesServiceLoadingTasks.Add(key, task);
            _ = this.InvokeOnExchangeRateLoaded(key);
            return task;
        }

        protected abstract Task<HEXToFiatCurrencyConverter> CreateLoadingTaskVirtual(string fromTicker, string toTicker);

        public HEXToFiatCurrencyConverter GetExchangeRate(string fromTicker, string toTicker)
        {
            var task = this.GetLoadingTask(fromTicker, toTicker);
            if (task.IsCompleted)
                return task.Result;
            return new HEXToFiatCurrencyConverter(Currencies.GetCurrency(toTicker), 0);
        }
        protected async Task InvokeOnExchangeRateLoaded(string key)
        {
            var converter = await this._exchangeRatesServiceLoadingTasks[key];
            AmountModelFactory.SetConverter(converter);
            this.OnExchangeRateLoaded?.Invoke(this, EventArgs.Empty);
        }
        public HEXToFiatCurrencyConverter GetExchangeRate()
        {
            return this.GetExchangeRate("HEX", "USD");
        }

        public virtual void ClearCache()
        {
            this._exchangeRatesServiceLoadingTasks.Clear();
        }

    }
    internal class ExchangeRatesService : BaseExchangeRateService
    {
        private readonly IJSInProcessRuntime jsRuntime;        
        private readonly ILogger logger;        
        
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        public ExchangeRatesService(IJSRuntime jsRuntime, ILogger<ExchangeRatesService> logger)
        {
            this.jsRuntime = (IJSInProcessRuntime)jsRuntime;
            this.logger = logger;
            this.moduleTask = new(this.InitializeAsync);
        }
        private async Task<IJSObjectReference> InitializeAsync()
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/exchange-rate-fetcher.js");
            return module;
        }
        protected override Task<HEXToFiatCurrencyConverter> CreateLoadingTaskVirtual(string fromTicker, string toTicker)
        {
            Task<HEXToFiatCurrencyConverter> task = this.LoadConverterFromScriptAsync(fromTicker, toTicker);
            return task;            
        }

        private async Task<HEXToFiatCurrencyConverter> LoadConverterFromScriptAsync(string fromTicker, string toTicker)
        {
            //Log("LoadConverterFromScriptAsync(" + fromTicker + ", " + toTicker + ") Loading from script.");
            var module = await this.moduleTask.Value;
            double factor = await module.InvokeAsync<double>("FetchRate", new object[] { fromTicker, toTicker });
            var converter = new HEXToFiatCurrencyConverter(Currencies.GetCurrency(toTicker), factor);
            return converter;
        }                     
     
        protected void Log(string message)
        {
            //logger.Log(LogLevel.Information, message);
        }
    }
   
    internal class ExchangeRateServiceStorageCached : ExchangeRatesService
    {
        private readonly IStorageProvider storage;
        public ExchangeRateServiceStorageCached(IJSRuntime jsRuntime, IStorageProvider storage, ILogger<ExchangeRatesService> logger) : base(jsRuntime, logger)
        {
            this.storage = storage;
        }

        protected override Task<HEXToFiatCurrencyConverter> CreateLoadingTaskVirtual(string fromTicker, string toTicker) => CreateLoadingTaskAsync(fromTicker, toTicker);
        
        private async Task<HEXToFiatCurrencyConverter> CreateLoadingTaskAsync(string fromTicker, string toTicker)
        {
            Log("ExchangeRateServiceStorageCached.CreateLoadingTaskAsync(" + fromTicker + ", " + toTicker + ")");
            string key = fromTicker + toTicker;

            var retrievalFromCurrentLevel = GetRetrievalTask(key);
            var converter = await retrievalFromCurrentLevel;            
            if(converter == null)
            {
                Log("ExchangeRateServiceStorageCached.GetLoadingTaskAsync(" + fromTicker + ", " + toTicker + ") converter == null");
                converter = await base.CreateLoadingTaskVirtual(fromTicker, toTicker);
                _ = this.storage.Store(key, new HEXToFiatCurrencyConverter.Serializable(converter), StorageExpirationPolicy.OneHour);
            }
            else
            {
                Log("ExchangeRateServiceStorageCached.GetLoadingTaskAsync(" + fromTicker + ", " + toTicker + ") Loaded from storage.");
            }
            return converter;
        }
        private async Task<HEXToFiatCurrencyConverter> GetRetrievalTask(string key)
        {
            Log("ExchangeRateServiceStorageCached.GetRetrievalTask(" + key + ")");
            var serializable = await this.storage.Retrieve<HEXToFiatCurrencyConverter.Serializable>(key);
            if (serializable == null)
                return null;
            return new HEXToFiatCurrencyConverter(Currencies.GetCurrency(serializable.Ticker), serializable.Factor);
        }
    }
}
