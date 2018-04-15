using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive
{
    partial class OliveExtensions
    {
        const double ACTUAL_DAYS_PER_YEAR = 365.2425;
        const int NINETEEN_HUNDRED = 1900;

        public static TimeSpan Days(this int number) => TimeSpan.FromDays(number);

        public static TimeSpan Hours(this int number) => TimeSpan.FromHours(number);

        public static TimeSpan Minutes(this int number) => TimeSpan.FromMinutes(number);

        public static TimeSpan Seconds(this int number) => TimeSpan.FromSeconds(number);

        public static TimeSpan Milliseconds(this int number) => TimeSpan.FromMilliseconds(number);

        public static TimeSpan Ticks(this int number) => TimeSpan.FromTicks(number);

        public static TimeSpan Multiply(this TimeSpan @this, double by) => TimeSpan.FromMilliseconds(@this.TotalMilliseconds * by);

        /// <summary>
        /// Gets the approximate number of the total years equivalent to this timespan.
        /// This is not accurate due to unknown leap years in the actual period to which this TimeSpan relates.
        /// </summary>
        public static double ApproxTotalYears(this TimeSpan @this) => @this.TotalDays / ACTUAL_DAYS_PER_YEAR;

        /// <summary>
        /// Converts this time to the date time on date of 1900-01-01.
        /// </summary>
        public static DateTime ToDate(this TimeSpan time) => new DateTime(NINETEEN_HUNDRED, 1, 1).Add(time);

        /// <summary>
        /// Converts this time to the date time on date of 1900-01-01.
        /// </summary>
        public static DateTime? ToDate(this TimeSpan? time) => time?.ToDate();

        /// <summary>
        /// Gets the natural text for this timespan. For example "2 days, 4 hours and 3 minutes".
        /// </summary>
        public static string ToNaturalTime(this TimeSpan period) => ToNaturalTime(period, longForm: true);

        public static string ToNaturalTime(this TimeSpan period, bool longForm) => ToNaturalTime(period, 2, longForm);

        public static string ToNaturalTime(this TimeSpan period, int precisionParts) =>
            ToNaturalTime(period, precisionParts, longForm: true);

        [EscapeGCop("It is ok for trying methods to have out param.")]
        static bool TryReduceDays(ref TimeSpan period, int len, out double result)
        {
            if (period.TotalDays >= len)
            {
                result = (int)Math.Floor(period.TotalDays / len);
                period -= TimeSpan.FromDays(len * result);

                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        /// Gets the natural text for this timespan. For example "2 days, 4 hours and 3 minutes".
        /// </summary>
        public static string ToNaturalTime(this TimeSpan period, int precisionParts, bool longForm)
        {
            // TODO: Support months and years.
            // Hint: Assume the timespan shows a time in the past of NOW. Count years and months from there.
            //          i.e. count years and go back. Then count months and go back...

            var names = new Dictionary<string, string> { { "year", "y" }, { "month", "M" }, { "week", "w" }, { "day", "d" }, { "hour", "h" }, { "minute", "m" }, { "second", "s" }, { " and ", " " }, { ", ", " " } };

            Func<string, string> name = (k) => longForm ? k : names[k];

            var parts = new Dictionary<string, double>();

            const int YEAR = 365, MONTH = 30, WEEK = 7;

            if (TryReduceDays(ref period, YEAR, out var years))
                parts.Add(name("year"), years);

            if (TryReduceDays(ref period, MONTH, out var months))
                parts.Add(name("month"), months);

            if (TryReduceDays(ref period, WEEK, out var weeks))
                parts.Add(name("week"), weeks);

            if (period.TotalDays >= 1)
            {
                parts.Add(name("day"), period.Days);
                period -= TimeSpan.FromDays(period.Days);
            }

            if (period.TotalHours >= 1 && period.Hours > 0)
            {
                parts.Add(name("hour"), period.Hours);
                period = period.Subtract(TimeSpan.FromHours(period.Hours));
            }

            if (period.TotalMinutes >= 1 && period.Minutes > 0)
            {
                parts.Add(name("minute"), period.Minutes);
                period = period.Subtract(TimeSpan.FromMinutes(period.Minutes));
            }

            if (period.TotalSeconds >= 1 && period.Seconds > 0)
            {
                parts.Add(name("second"), period.Seconds);
                period = period.Subtract(TimeSpan.FromSeconds(period.Seconds));
            }

            else if (period.TotalSeconds > 0)
            {
                parts.Add(name("second"), period.TotalSeconds.Round(3));
                period = TimeSpan.Zero;
            }

            var outputParts = parts.Take(precisionParts).ToList();
            var r = new StringBuilder();

            foreach (var part in outputParts)
            {
                r.Append(part.Value);

                if (longForm) r.Append(" ");

                r.Append(part.Key);

                if (part.Value > 1 && longForm) r.Append("s");

                if (outputParts.IndexOf(part) == outputParts.Count - 2)
                    r.Append(name(" and "));
                else if (outputParts.IndexOf(part) < outputParts.Count - 2)
                    r.Append(name(", "));
            }

            return r.ToString();
        }

        public static string ToString(this TimeSpan? value, string format) => ("{0:" + format + "}").FormatWith(value);

        public static int CompareTo(this TimeSpan? @this, TimeSpan? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this == TimeSpan.Zero ? 1 : @this.Value.CompareTo(TimeSpan.Zero);
            if (@this == null) return another == TimeSpan.Zero ? -1 : another.Value.CompareTo(TimeSpan.Zero);
            return @this.Value.CompareTo(another.Value);
        }
    }
}