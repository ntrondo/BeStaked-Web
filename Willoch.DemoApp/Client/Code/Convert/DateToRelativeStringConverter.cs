using System;
using System.Linq;
using UtilitiesLib.ConvertPrimitives.Interfaces;

namespace Willoch.DemoApp.Client.Code.Convert
{
    public class DateToRelativeStringConverter : IConvert<DateTime, string>
    {
        private DateTime BenchMark { get; }

        public string Convert(DateTime date)
        {
            if(date.Year == 1)
                return String.Empty;
            return TimeSpanToDisplayString(date - this.BenchMark);
        }
        public DateToRelativeStringConverter()
        {
            this.BenchMark = DateTime.UtcNow;
        }
        private static readonly Tuple<Func<TimeSpan, bool>, string, Func<TimeSpan, double>>[] convertersByUnit = new Tuple<Func<TimeSpan, bool>, string, Func<TimeSpan, double>>[]
        {
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=>s.TotalMinutes < 2), "seconds", new Func<TimeSpan, double>(s=>s.TotalSeconds)),
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=>s.TotalHours < 2), "minutes", new Func<TimeSpan, double>(s=>s.TotalMinutes)),
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=>s.TotalDays < 2),"hours", new Func<TimeSpan, double>(s=>s.TotalHours)),
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=>s.TotalDays < 14),"days", new Func<TimeSpan, double>(s=>s.TotalDays)),
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=>s.TotalDays < 60),"weeks", new Func<TimeSpan, double>(s=>s.TotalDays/7)),
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=>s.TotalDays < 30 * 12 * 2),"months", new Func<TimeSpan, double>(s=>s.TotalDays/30)),
            new Tuple<Func<TimeSpan, bool>,string, Func<TimeSpan, double>>(new Func<TimeSpan, bool>(s=> true),"years", new Func<TimeSpan, double>(s=>s.TotalDays/365))
        };

        private static string TimeSpanToDisplayString(TimeSpan ts)
        {            
            if (ts == TimeSpan.Zero)
                return "now";
            var d = ts.Duration();
            var converter = convertersByUnit.First(c => c.Item1(d));
            var durationString = String.Format("{0} {1}", Math.Round(converter.Item3(d)), converter.Item2);
            if (ts.TotalMilliseconds < 0)
                return string.Format("{0} ago", durationString);
            return durationString;
        }
    }
}
