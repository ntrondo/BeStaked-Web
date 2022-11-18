using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesLib.ConvertPrimitives.Implementations.Double;
using UtilitiesLib.ConvertPrimitives.Interfaces;
using UtilitiesLib.Models.Implementations;
using UtilitiesLib.Models.Interfaces;
using UtilitiesLibBeStaked.Factories;
using Willoch.DemoApp.Client.Code.Calculate;
using Willoch.DemoApp.Client.Code.Convert;
using Willoch.DemoApp.Client.Code.Models.Amounts;
using Willoch.DemoApp.Client.Services;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Shared.Stake
{
    public enum FeeSource
    {
        Contract=0, Site=1, Total=5
    }
    public class FeeLine
    {
 
        public FeeSource Source { get;}
        public IComplexAmountModel Percentage { get; set; }
        public IComplexConvertedAmountModel Amount { get; set; }

        public FeeLine(FeeSource source)
        {
            this.Source = source;
        }
    }
    public interface IBSFeeEstimatesModel
    {
        string PrimarySymbol { get; }
        string ForeignSymbol { get; }
        string Label { get; }
        FeeLine[] Lines { get; }
        IConvert<double, ICurrencyAmount> Exchanger { get; }
        void SetExchanger(IConvert<double, ICurrencyAmount> exchanger);
        /// <summary>
        /// Update fee by providing the stake type, fee source and a number between 0 and 100
        /// </summary>
        /// <param name="st">stake type</param>
        /// <param name="fs">fee source</param>
        /// <param name="fee">[0, 100]</param>
        void SetFee(StakeType st, FeeSource fs, double fee);
      
        double GetExFees(double inclFees);
        bool AnyFees { get; }
        bool AnyAmount { get; }
        /// <summary>
        /// Display fees based on this principal
        /// </summary>
        /// <param name="amountModel"></param>
        void ReceiveAmount(IAmountInput amountModel);
        event EventHandler OnOutputChanged;
    }


    public abstract class BaseBSFeeEstimateModel : IBSFeeEstimatesModel
    {
        public string ForeignSymbol => AmountModelFactory.Create(0).Converted.Original.Currency.SymbolOrTicker;
        public string PrimarySymbol => AmountModelFactory.Create(0).Original.Currency.SymbolOrTicker;
        private static IConvert<double, string> PercentageFormatter = new PercentageToDisplayStringConverter(3, RoundingMode.ToNearest);
        private static IConvert<double, string> amountFormatter = ConstrainedDoubleAmountToShortStringConverter.Instance;
        public string Label { get; }


        public FeeLine[] Lines { get; private set; } = new FeeLine[0];
        public IConvert<double, ICurrencyAmount> Exchanger {get;private set;}
        public void SetExchanger(IConvert<double, ICurrencyAmount> exchanger)
        {
            this.Exchanger = exchanger;
            if(AnyFees && AnyAmount)
            {
                this.RecalculateLines();
                this.OnOutputChanged?.Invoke(this, null);
            }
                      
        }
        private IStakeTypeProvider TypeSelector { get; }
       
        private Dictionary<StakeType, Dictionary<FeeSource, double>> Fees = new();
        public void SetFee(StakeType st, FeeSource fs, double fee)
        {
            if (!Fees.ContainsKey(st))
                Fees.Add(st, new Dictionary<FeeSource, double>());
            var fees = Fees[st];
            bool add = !fees.ContainsKey(fs);
            bool change = add || fees[fs] != fee;
            if (!change)
                return;
            if (add)
                fees.Add(fs, fee);
            else
                fees[fs] = fee;
            if(this.TypeSelector.Value == st)
            {
                this.RecalculateLine(fs);
                this.OnOutputChanged?.Invoke(this, null);
            }                          
        }
        /// <summary>
        /// Recalculates all lines in order. Does not raise any events.
        /// </summary>
        private void RecalculateLines()
        {
            var sources = (FeeSource[])Enum.GetValues(typeof(FeeSource));
            foreach (var source in sources)
                RecalculateLine(source, false);
            //Remove zero fee lines
            this.Lines = this.Lines.Where(l => l.Percentage.Amount > 0).ToArray();
            if (Lines.Count(l => l.Source != FeeSource.Total) <= 1)
                //Remove total line
                Lines = Lines.Where(l => l.Source != FeeSource.Total).ToArray();
        }
        private void RecalculateLine(FeeSource fs, bool recalculateTotal = true)
        {
            var line = Lines.FirstOrDefault(l => l.Source == fs);
            if(line == null)
            {
                line = new FeeLine(fs);
                var list = Lines.ToList();
                list.Add(line);
                Lines = list.OrderBy(l => l.Source).ToArray();
            }
            RecalculateLine(line, recalculateTotal);
        }

        private void RecalculateLine(FeeLine line, bool recalculateTotal = true)
        {
            var st = TypeSelector.Value;
            var fees = Fees.ContainsKey(st) ? Fees[st] : null;
            double fee = fees == null ? 0 : 
                (line.Source == FeeSource.Total ? fees.Sum(p => p.Value) : ( fees.ContainsKey(line.Source) ? fees[line.Source]:0));
            line.Percentage = AmountModelFactory.Create(fee);
            line.Amount = AmountModelFactory.Create(CalculateFee(line.Percentage, AmountModel?.Amount));
            if (recalculateTotal && line.Source != FeeSource.Total)
                RecalculateLine(FeeSource.Total, false);
        }

        public event EventHandler OnOutputChanged;


        public bool AnyFees => this.Fees.ContainsKey(TypeSelector.Value) && this.Fees[TypeSelector.Value].Any(p=>p.Value > 0);

        

        public BaseBSFeeEstimateModel(string label, IStakeTypeProvider typeSelector)
        {
            this.Label = label;
            this.TypeSelector = typeSelector;
            this.TypeSelector.OnValueChanged += TypeSelector_OnValueChanged;
        }

        private void TypeSelector_OnValueChanged(object sender, System.EventArgs e)
        {
            RecalculateLines();
            this.OnOutputChanged?.Invoke(this, null);
        }

        public double GetExFees(double inclFees)
        {
            var st = this.TypeSelector.Value;
            if (!Fees.ContainsKey(st))
                return inclFees;
            double totalFee = Fees[st].Sum(p => p.Value);
            if (totalFee == 0)
                return inclFees;
            return inclFees * (1 - totalFee / 100);
        }

        private static double CalculateFee(IComplexAmountModel feePercentageModel, IComplexAmountModel exFeesAmountModel)
        {
            double percentage = feePercentageModel == null ? 0 : feePercentageModel.Amount;
            double exFees = exFeesAmountModel == null ? 0 : exFeesAmountModel.Amount;
            if (exFees == 0 || percentage == 0)
                return 0;
            double factor = percentage / (100 + percentage);
            return exFees * factor;
        }
        private IAmountInput AmountModel { get; set; }
        public bool AnyAmount => AmountModel != null && AmountModel.Amount.Amount > 0;


      

        public void ReceiveAmount(IAmountInput amountModel)
        {
            if(this.AmountModel != null)            
                this.AmountModel.OnAmountChanged-=AmountModel_OnAmountChanged;            
            this.AmountModel = amountModel;
            this.AmountModel.OnAmountChanged += AmountModel_OnAmountChanged;
            this.AmountModel_OnAmountChanged(this, EventArgs.Empty);
        }
        private void AmountModel_OnAmountChanged(object sender, EventArgs e)
        {
            if (!AnyFees)
                return;
            this.RecalculateLines();
            this.OnOutputChanged(this, EventArgs.Empty);
        }
    }
    public class BSFeeEstimateModel : BaseBSFeeEstimateModel
    {
        public BSFeeEstimateModel(IStakeTypeProvider typeSelector) :base("Fees", typeSelector)
        {  }
    }
    public partial class BSFeeEstimates : ComponentBase
    {
        [Parameter, EditorRequired]
        public IBSFeeEstimatesModel Model { get; set; }
        [Inject]
        private ILogger<BSFeeEstimates> logger { get; set; }
        [Inject]
        private ITransferableStakeAccessor TransferableStakeAsyncAccessor { get; set; }

        protected override Task OnInitializedAsync()
        {
            this.Model.OnOutputChanged += Model_OnOutputChanged;
            _ = this.LoadFees();
            return base.OnInitializedAsync();
        }

        private Task<double> FeeLoadingTask = null;
        private async Task LoadFees()
        {
            logger.Log(LogLevel.Information, "LoadFees()");
            if(this.FeeLoadingTask == null)
            {
                this.Model.SetFee(StakeType.Transferable, FeeSource.Site, Constants.SiteFeePermille / 10.0);
                this.FeeLoadingTask = TransferableStakeAsyncAccessor.GetContractFeePercentage();
                ///[0, 100]
                double contractFeePercentage = await this.FeeLoadingTask;
                logger.Log(LogLevel.Information, "LoadFees() percentage=" + contractFeePercentage);
                this.Model.SetFee(StakeType.Transferable, FeeSource.Contract, contractFeePercentage);
                this.FeeLoadingTask = null;
            }
        }

        private void Model_OnOutputChanged(object sender, EventArgs e)
        {
            this.StateHasChanged();
        }
    }
}
