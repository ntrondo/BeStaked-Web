using UtilitiesLib.ConvertPrimitives.BaseClasses;

namespace UtilitiesLibBeStaked.Converters
{
    public interface IBonusAmountCalculator
    {
        double GetResult(double amount, int duration);
    }
    public class BonusAmountCalculator : CachedConverterBase<SharesCalculator.InputModel, double>
    {       
        private static BonusAmountCalculator? _instance;
        public static BonusAmountCalculator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BonusAmountCalculator();
                return _instance;
            }
        }
        private static readonly Dictionary<Tuple<double, int>, double> _cache = new();
        private BonusAmountCalculator() { }
        static int LPB_BONUS_PERCENT = 20;
        static int LPB_BONUS_MAX_PERCENT = 200;
        static int LPB = 364 * 100 / LPB_BONUS_PERCENT;//1820

        static int BPB_MAX_HEX = (int)(150 * 1e6);
        static int BPB_BONUS_PERCENT = 10;
        private static double CalculateBonusAmountFromContract(double amount, int duration)
        {
            //Source: _stakeStartBonusHearts
            int cappedExtraDays = Math.Min(duration - 1, LPB * LPB_BONUS_MAX_PERCENT / 100/*3640*/);
            double cappedStakedHex = amount <= BPB_MAX_HEX ? amount : BPB_MAX_HEX;
            double BPB = (double)BPB_MAX_HEX * 100 / BPB_BONUS_PERCENT;
            double bonusHex = cappedExtraDays * BPB + cappedStakedHex * LPB;
            bonusHex = amount * bonusHex / (LPB * BPB);

            return bonusHex;
        }

        public override double convert(SharesCalculator.InputModel value) => CalculateBonusAmountFromContract(value.Principal, value.Duration);
    }
}
