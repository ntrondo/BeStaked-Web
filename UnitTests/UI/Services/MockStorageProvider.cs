using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willoch.DemoApp.Client.Services;

namespace UnitTests.UI.Services
{
    internal class MockStorageProvider : IStorageProvider
    {
        public Task<bool> ContainsKey(object key)
        {
            throw new NotImplementedException();
        }

        public Task Remove(object key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAll()
        {
            throw new NotImplementedException();
        }

        public Task<T> Retrieve<T>(object key) where T : new()
        {
            throw new NotImplementedException();
        }

        public Task Store<T>(object key, T item, DateTime expires)
        {
            throw new NotImplementedException();
        }

        public Task Store<T>(object key, T item, StorageExpirationPolicy policy)
        {
            throw new NotImplementedException();
        }
    }
}
