using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willoch.DemoApp.Shared;

namespace Willoch.DemoApp.Client
{
    public static class Constants
    {
        public const string Brand = "BeStaked";
        public const double SiteFeePermille = 0;

        public static readonly Dictionary<int, NetworkInfo> Networks = new()
        {
            [1111] = new(1111, "private", "DEX", "dev", new ("0xE3F932E173127B1B873474Bd06AcBe058E652Eb1", 8) , new(""), ""),
            [3] = new(3, "ropsten", "TEX", "test", new("0xf1633e8d441f6f5e953956e31923f98b53c9fd89", 8), new("0x53F840d990d7c88b90A5496b7470Ee0b01083Ed8"), ""),
            [1] = new(1, "main", "HEX", "prod", new("0x2b591e99afe9f32eaa6214f7b7629768c40eeb39", 8), new("0x22E1A96E3103AC7a900DF634d0E2696D05100856"), ""),
            [11155111] = new(11155111, "sepolia", "TEX", "test", new("0xCE325889177a36aD87C7311568e810Ada6493779", 8), new("0xAdd3923b47557DADE1af4E70065ab0aDf3240813"), "")

        };
    }
}
