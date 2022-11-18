using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.Models.Implementations;
using UtilitiesLibBeStaked.Factories;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public class AmountInputModel : AmountInputModelBase
    {
        public override string Symbol => AmountModelFactory.Create(0).Original.Currency.SymbolOrTicker;
                      
        public override IComplexAmountModel Amount { get => ConvertedAmount; }
        private IComplexConvertedAmountModel ConvertedAmount { get; set; }      
        public AmountInputModel(double min, double max)
            :base("Principal", min, max)
        {            
            this.ConvertedAmount = AmountModelFactory.Create(min);
        }
        public AmountInputModel()
            :this(default,default)
        {  }
        public override event EventHandler OnAmountChanged;
        public override void SetAmount(double amount)
        {            
            bool change = this.ConvertedAmount == null || this.ConvertedAmount.Amount != amount;
            this.ConvertedAmount = AmountModelFactory.Create(amount);
            if (change)
            {
                OnAmountChanged?.Invoke(this, null);                
            }                
        }
        protected override void ExchangerChanged()
        {
            base.ExchangerChanged();
            var amount = this.Amount.Amount;
            this.ConvertedAmount = null;
            this.SetAmount(amount);
        }
    }
    public class AmountInputModelFeeAdjusted: AmountInputModel
    {
        public IBSFeeEstimatesModel FeesModel { get; }
        public AmountInputModelFeeAdjusted(IBSFeeEstimatesModel feesModel)
            : this(default, default, feesModel)
        { }
        public AmountInputModelFeeAdjusted(double min, double max, IBSFeeEstimatesModel feesModel) 
            : base(min, max)
        {
            this.ReceivedRange = new double[] { min, max };
            this.FeesModel = feesModel;
            this.FeesModel.OnOutputChanged += FeesModel_OnOutputChanged;
        }
        private void FeesModel_OnOutputChanged(object sender, EventArgs e) => this.AdjustRangeAndCallBase();
        private readonly double[] ReceivedRange;
        public override void SetRange(double min, double max)
        {
            this.ReceivedRange[0] = min;
            this.ReceivedRange[1] = max;
            AdjustRangeAndCallBase();
        }

        private void AdjustRangeAndCallBase()
        {
            double min = ReceivedRange[0];
            double max = ReceivedRange[1];

            double adjustedMin = min;
            double adjustedMax = FeesModel.GetExFees(max);// * (1 - FeesModel.SiteFeePercentage.Amount - FeesModel.ContractFeePercentage.Amount);

            base.SetRange(adjustedMin, adjustedMax);
        }
    }
    public partial class AmountInput : NumericInputComponentBase
    {
        [Inject]
        private Services.IAssetsService AssetsService { get; set; }
        [Inject]
        private ILogger<AmountInput> Logger { get; set; }
        protected override RelativeAmountOptionModel[] GenerateOptions()
        {
            double max = this.Model.Maximum.Amount;
            int decimals = -(int)Math.Round(Math.Log10(max) / 2);
            IConvert<double, string> formatter = ConstrainedDoubleAmountToShortStringConverter.Instance;
            ITransform<double> rounder = new DoubleRounder(decimals, RoundingMode.TowardsZero);
            var list = new List<RelativeAmountOptionModel>();
            
            for (int i = 1; i < 11; i++)
            {
                string label = string.Format("≈ {0} %", Math.Round(100.0 / i));
                double amount = rounder.Convert(max / i);
                if (amount <= this.Model.Minimum.Amount)
                    break;
                list.Add(new RelativeAmountOptionModel(label, AmountModelFactory.Create(amount)));
            }
            RelativeAmountOptionModel[] array;
            {
                list.Add(new RelativeAmountOptionModel("= 100 %", AmountModelFactory.Create(max)));
                int bpb = (int)150e6;
                if (bpb < max)
                    list.Add(new RelativeAmountOptionModel(string.Format("= Bonus max"), AmountModelFactory.Create(bpb)));
                array = list.OrderByDescending(o => o.Amount.Amount).ToArray();
            }           
            return array;
        }
        protected override Task OnInitializedAsync()
        {
            this.AssetsService.DataRefreshed += AssetsService_DataRefreshed;
            AssetsService_DataRefreshed(null, null);
            return base.OnInitializedAsync();
        }

        private void AssetsService_DataRefreshed(object sender, EventArgs e)
        {
            var newBalance = this.AssetsService.WalletAssets == null ? default : this.AssetsService.WalletAssets.StakeableBalance;
            this.Model.SetRange(default, newBalance);
        }

        protected override void Log(string message)
        {
            Logger.Log(LogLevel.Information, message);
        }
    }    
}
