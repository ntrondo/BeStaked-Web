using System;

namespace Willoch.DemoApp.Client.Code.Models
{
    public class StakeValuation
    {
        public DateTime FirstDay { get; }
        /// <summary>
        /// Last active date of the stake. The day before maturity.
        /// </summary>
        public DateTime LastDay { get; }
        public double AccruedInterest { get; }
        public double DailyInterest { get; }
        public double BookValue { get; }
        public double MarketValue { get; }
        public double Reward { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accruedInterest"></param>
        /// <param name="dailyInterest"></param>
        /// <param name="bookValue"></param>
        /// <param name="marketValue"></param>
        /// <param name="firstDay"></param>
        /// <param name="lastDay">Last active date of the stake. The day before maturity.</param>
        public StakeValuation(double accruedInterest, double dailyInterest, double bookValue, double marketValue, DateTime firstDay, DateTime lastDay, double reward)
        {
            AccruedInterest = accruedInterest;
            DailyInterest = dailyInterest;
            BookValue = bookValue;
            this.MarketValue = marketValue;

            FirstDay = firstDay;
            LastDay = lastDay;
            this.Reward = reward;
        }
    }
}
