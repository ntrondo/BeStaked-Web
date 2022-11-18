using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace UnitTests.UI.Services
{
    internal class MockLogger<T> : ILogger<T>
    {
        private readonly string CategoryName;
        private ITestOutputHelper Output { get; }
        internal MockLogger(ITestOutputHelper output)
        {
            this.Output = output;
            this.CategoryName = typeof(T).Name;
        }
        

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = String.Empty;
            if (formatter != null)
            {
                message += formatter(state, exception);
            }
            if(eventId == 0)
            Output.WriteLine($"{logLevel.ToString()} <{CategoryName}> {message}");
            else
            Output.WriteLine($"{logLevel.ToString()} - {eventId.Id} <{CategoryName}> {message}");
        }
    }
}
