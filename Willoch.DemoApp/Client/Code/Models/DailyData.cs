using System;
using System.Numerics;
using Willoch.DemoApp.Shared;
using Willoch.DemoApp.Shared.Utilities;

namespace Willoch.DemoApp.Client.Code.Models
{
    public class DailyData
    {
        [System.Text.Json.Serialization.JsonPropertyName("d")]
        public int Day { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("f")]
        public double Factor { get; set; }
        public DailyData(int day, string[] hexValues,  ERC20Info contractInfo)
        {
            this.Day = day;
            var payout = Numbers.ConvertToTokenAmount(hexValues[0], contractInfo);
            BigInteger sharesBI = Numbers.ParseToBigInteger(hexValues[1]);
            var shares = (double)sharesBI;
            this.Factor = shares == 0 ? 0 : payout / shares;               
        }
        public DailyData() { }
        public double GetPayout(UInt64 shares)
        {
            return this.Factor * shares;          
        }
    }
   
}
