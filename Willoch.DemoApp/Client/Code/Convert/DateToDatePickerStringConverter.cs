using System;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace Willoch.DemoApp.Client.Code.Convert
{
    public class DateToDatePickerStringConverter : IConvert<DateTime, string>
    {
        public string Convert(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}
