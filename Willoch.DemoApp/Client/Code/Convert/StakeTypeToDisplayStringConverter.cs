using UtilitiesLib.ConvertPrimitives.Interfaces;
using Willoch.DemoApp.Client.Shared.Stakes;

namespace Willoch.DemoApp.Client.Code.Convert
{
    public class StakeTypeToSingularDisplayStringConverter : IConvert<StakeType, string>
    {
        public virtual string Convert(StakeType st)
        {
            return st switch
            {
                StakeType.Legacy => "Regular stake",
                _ => string.Format("{0} stake", st),
            };
        }

        //public string Convert(object value)
        //{
        //    if (value is StakeType st)
        //        return Convert(st);
        //    return value.ToString();
        //}
    }
    public class StakeTypeToPluralDisplayStringConverter:StakeTypeToSingularDisplayStringConverter
    {
        public override string Convert(StakeType st)
        {
            return base.Convert(st) + "s";
        }
    }
}
