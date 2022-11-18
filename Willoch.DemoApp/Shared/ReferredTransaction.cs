using System;

namespace Willoch.DemoApp.Shared
{
    public class ReferredTransaction
    {
        public string TransactionId { get; set; }
        public string ReferrerCode { get; set; }
        public string Account { get; set; }
        public int Principal { get; }
        public DateTime UtcTimestamp { get; }
    }
}
