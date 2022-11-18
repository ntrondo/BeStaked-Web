using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesLib.Models.Implementations;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Pages
{
    public partial class Sepolia:ComponentBase
    {
        [Inject]
        private IStakeableAsyncAccessor StakeableAsyncAccessor { get; set; }
        [Inject]
        private Services.IAssetsService AssetsService { get; set; }
        private double Amount;
        private IComplexAmountModel[] Amounts { get; set; } = new IComplexAmountModel[0];
        protected override Task OnInitializedAsync()
        {
            Amounts = GenerateAmounts();
            Amount = Amounts.First().Amount;
            return base.OnInitializedAsync();
        }

        private IComplexAmountModel[] GenerateAmounts()
        {
            var r = new Random();
            var baseAmounts = new double[] { 1e2, 1e4, 1e6, 1e8, 1e10, 1e12, 1e14 };
            return baseAmounts.Select(i => (IComplexAmountModel)AmountModelFactory.Create(r.NextDouble() * i)).OrderBy(a => a.Original.Amount).ToArray();
        }
        private void OnAmountChanged(ChangeEventArgs e)
        {
            var amount = double.Parse(e.Value.ToString());
            this.Amount = amount;
        }
        private Task<bool> SendMintTransactionTask;
        private async void MintClicked()
        {
            if (SendMintTransactionTask != null)
                return;
            SendMintTransactionTask = StakeableAsyncAccessor.Mint(Amount);
            bool completed = await SendMintTransactionTask;
            if (completed)
                AssetsService.AssumeTransfer(Amount);
            SendMintTransactionTask = null;
        }
    }
}
