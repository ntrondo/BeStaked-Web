using System;
using Willoch.DemoApp.Shared;

namespace Willoch.DemoApp.Client.Code.Models
{
    public class StakeInfo
    {
        public UInt64 StakeId { get; set; }
        public double StakedAmount { get; set; }
        public UInt16 LockedDay { get; set; }
        public UInt64 Shares { get; set; }
        public UInt16 StakedDays { get; set; }
        public UInt16 UnlockedDay { get; set; }
        public bool IsAutoStake { get; set; }
        public string Summary
        {
            get
            {
                string verb = (IsAutoStake ? "Autos" : "S") + "tak" + (LockedDay == 0 ? "ing" : "ed") + " {0} tokens";
                string on = (LockedDay == 0 ? string.Empty : " on day {1}");
                string ford = " for {2} days.";
                string status = " Stake " + (LockedDay == 0 ? "is pending" : string.Empty) + (LockedDay > 0 && UnlockedDay == 0 ? "is active" : string.Empty) + (UnlockedDay > 0 ? "ended on day {3}" : string.Empty) + ".";
                return string.Format(verb + on + ford + status, (int)StakedAmount, LockedDay, StakedDays, UnlockedDay);
            }
        }
        public StakeInfo() { }
        public StakeInfo(string[] hexValues, ERC20Info contractInfo)
        {
            this.StakeId = System.Convert.ToUInt64(hexValues[0], 16);
            this.StakedAmount = DemoApp.Shared.Utilities.Numbers.ConvertToTokenAmount(hexValues[1], contractInfo);
            this.Shares = System.Convert.ToUInt64(hexValues[2], 16);
            this.LockedDay = System.Convert.ToUInt16(hexValues[3], 16);
            this.StakedDays = System.Convert.ToUInt16(hexValues[4], 16);
            this.UnlockedDay = System.Convert.ToUInt16(hexValues[5], 16);
            this.IsAutoStake = System.Convert.ToBoolean(System.Convert.ToInt16(hexValues[6], 16));
        }

        public StakeInfo(ulong stakeId, double stakedAmount, ulong shares, ushort lockedDay, ushort stakedDays, ushort unlockedDay, bool isAutoStake)
        {
            StakeId = stakeId;
            StakedAmount = stakedAmount;
            Shares = shares;
            LockedDay = lockedDay;
            StakedDays = stakedDays;
            UnlockedDay = unlockedDay;
            IsAutoStake = isAutoStake;
        }
    }
    public class TStakeInfo:StakeInfo
    {
        public UInt64 TransferableStakeId { get; }
        public ushort RewardStretching { get; }

        public TStakeInfo(string[] hexValues, ERC20Info contractInfo):base(hexValues, contractInfo) { }
        public TStakeInfo(UInt64 transferrableStakeId, StakeInfo s, UInt16 rewardStretching)
            :base(s.StakeId, s.StakedAmount, s.Shares, s.LockedDay, s.StakedDays, s.UnlockedDay, s.IsAutoStake)
        {
            this.TransferableStakeId = transferrableStakeId;
            this.RewardStretching = rewardStretching;
        }
    }
}
