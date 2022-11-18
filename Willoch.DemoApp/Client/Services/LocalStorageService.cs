using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Willoch.DemoApp.Client.Services
{
    public enum StorageExpirationPolicy
    {
        FiveMinutes,
        OneHour,
        UntilMidnightUTC,
        Infinite
    }
    public interface IStorageProvider
    {
        Task Store<T>(object key, T item, DateTime expires);
        Task Store<T>(object key, T item, StorageExpirationPolicy policy);
        Task<bool> ContainsKey(object key);
        Task<T> Retrieve<T>(object key) where T : new();
        Task Remove(object key);
        Task RemoveAll();
    }
    public class LocalStorageService : IStorageProvider
    {
        private readonly IJSInProcessRuntime jsRuntime;
        private readonly ILogger logger;
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        public LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
        {
            this.jsRuntime = (IJSInProcessRuntime)jsRuntime;
            this.logger = logger;
            this.moduleTask = new(this.InitializeAsync);
        }
        private async Task<IJSObjectReference> InitializeAsync()
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/local-storage-proxy.js");
            return module;
        }
        public async Task<T> Retrieve<T>(object key) where T : new()
        {
            string keyString = JsonSerializer.Serialize(key);
            if (string.IsNullOrEmpty(keyString))
                return default;
            var module = await this.moduleTask.Value;
            string serialValue = await module.InvokeAsync<string>("Retrieve", new object[] { keyString });
            if (string.IsNullOrEmpty(serialValue))
                return default;
            var wrapper = JsonSerializer.Deserialize<StorageWrapper>(serialValue);
            if(wrapper != null && wrapper.Expiration <= DateTime.Now)
            {
                await Remove(key);
                wrapper = null;
            }
            if(string.IsNullOrEmpty(wrapper?.Data))
                return default;            
            var value = JsonSerializer.Deserialize<T>(wrapper.Data);
            return value;
        }

        public async Task Store<T>(object key, T item, DateTime expires)
        {
            string keyString = JsonSerializer.Serialize(key);
            if (string.IsNullOrEmpty(keyString))
                return;
            var wrapper = new StorageWrapper();
            wrapper.Expiration = expires;
            wrapper.Data = JsonSerializer.Serialize(item);
            var serialValue = JsonSerializer.Serialize(wrapper);
            var module = await this.moduleTask.Value;
            await module.InvokeVoidAsync("Store", new object[] { keyString, serialValue });            
        }

        public Task Store<T>(object key, T item, StorageExpirationPolicy policy)
        {
            DateTime expires;
            switch(policy)
            {
                case StorageExpirationPolicy.FiveMinutes:expires = DateTime.Now.AddMinutes(5);break;
                case StorageExpirationPolicy.OneHour: expires = DateTime.Now.AddHours(1);break;
                case StorageExpirationPolicy.UntilMidnightUTC: expires = DateTime.UtcNow.Date.AddDays(1).ToLocalTime();break;
                default: expires = DateTime.Today.AddYears(1);break;
            }
            return Store(key, item, expires);
        }

        public async Task Remove(object key)
        {
            string keyString = JsonSerializer.Serialize(key);
            if (string.IsNullOrEmpty(keyString))
                return;
            var module = await this.moduleTask.Value;
            await module.InvokeVoidAsync("Remove", new object[] { keyString });
        }

        public async Task RemoveAll()
        {
            var module = await this.moduleTask.Value;
            await module.InvokeVoidAsync("RemoveAll");
        }

        public async Task<bool> ContainsKey(object key)
        {
            string keyString = JsonSerializer.Serialize(key);
            var module = await this.moduleTask.Value;
            string serialValue = await module.InvokeAsync<string>("Retrieve", new object[] { keyString });
            if (string.IsNullOrEmpty(serialValue))
            {
                logger.Log(LogLevel.Information, "ContainsKey(" + keyString + ") => false");
                return false;
            }                
            var wrapper = JsonSerializer.Deserialize<StorageWrapper>(serialValue);
            if (wrapper?.Data != null && wrapper.Expiration > DateTime.Now)
                return true;
            return false;
        }

        private class StorageWrapper
        {
            [JsonPropertyName("d")]
            public string Data { get; set; }
            [JsonPropertyName("e")]
            public DateTime Expiration { get; set; }
        }
    }
}
