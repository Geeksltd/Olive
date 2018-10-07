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

        [EscapeGCop("I AM the solution myself!")]
        public static TimeSpan Days(this int number) => TimeSpan.FromDays(number);

        [EscapeGCop("I AM the solution myself!")]
        public static TimeSpan Hours(this int number) => TimeSpan.FromHours(number);

        [EscapeGCop("I AM the solution myself!")]
        public static TimeSpan Minutes(this int number) => TimeSpan.FromMinutes(number);

        [EscapeGCop("I AM the solution myself!")]
        public static TimeSpan Seconds(this int number) => TimeSpan.FromSeconds(number);

        [EscapeGCop("I AM the solution myself!")]
        public static TimeSpan Milliseconds(this int number) => TimeSpan.FromMilliseconds(number);

        [EscapeGCop("I AM the solution myself!")]
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
        public static DateTime ToDate(this TimeSpan @this) => new DateTime(NINETEEN_HUNDRED, 1, 1).Add(@this);

        /// <summary>
        /// Converts this time to the date time on date of 1900-01-01.
        /// </summary>
        public static DateTime? ToDate(this TimeSpan? @this) => @this?.ToDate();

        /// <summary>
        /// Gets the natural text for this timespan. For example "2 days, 4 hours and 3 minutes".
        /// </summary>
        public static string ToNaturalTime(this TimeSpan @this) => ToNaturalTime(@this, longForm: true);

        public static string ToNaturalTime(this TimeSpan @this, bool longForm) => ToNaturalTime(@this, 2, longForm);

        public static string ToNaturalTime(this TimeSpan @this, int precisionParts) =>
            ToNaturalTime(@this, precisionParts, longForm: true);

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
        public static string ToNaturalTime(this TimeSpan @this, int precisionParts, bool longForm)
        {
            // TODO: Support months and years.
            // Hint: Assume the timespan shows a time in the past of NOW. Count years and months from there.
            //          i.e. count years and go back. Then count months and go back...

            var names = new Dictionary<string, string> { { "year", "y" }, { "month", "M" }, { "week", "w" }, { "day", "d" }, { "hour", "h" }, { "minute", "m" }, { "second", "s" }, { " and ", " " }, { ", ", " " } };

            Func<string, string> name = (k) => longForm ? k : names[k];

            var parts = new Dictionary<string, double>();

            const int YEAR = 365, MONTH = 30, WEEK = 7;

            if (TryReduceDays(ref @this, YEAR, out var years))
                parts.Add(name("year"), years);

            if (TryReduceDays(ref @this, MONTH, out var months))
                parts.Add(name("month"), months);

            if (TryReduceDays(ref @this, WEEK, out var weeks))
                parts.Add(name("week"), weeks);

            if (@this.TotalDays >= 1)
            {
                parts.Add(name("day"), @this.Days);
                @this -= @this.Days.Days();
            }

            if (@this.TotalHours >= 1 && @this.Hours > 0)
            {
                parts.Add(name("hour"), @this.Hours);
                @this = @this.Subtract(@this.Hours.Hours());
            }

            if (@this.TotalMinutes >= 1 && @this.Minutes > 0)
            {
                parts.Add(name("minute"), @this.Minutes);
                @this = @this.Subtract(@this.Minutes.Minutes());
            }

            if (@this.TotalSeconds >= 1 && @this.Seconds > 0)
            {
                parts.Add(name("second"), @this.Seconds);
                @this = @this.Subtract(@this.Seconds.Seconds());
            }

            else if (@this.TotalSeconds > 0)
            {
                parts.Add(name("second"), @this.TotalSeconds.Round(3));
                @this = TimeSpan.Zero;
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

        public static string ToString(this TimeSpan? @this, string format) => ("{0:" + format + "}").FormatWith(@this);

        public static int CompareTo(this TimeSpan? @this, TimeSpan? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this == TimeSpan.Zero ? 1 : @this.Value.CompareTo(TimeSpan.Zero);
            if (@this == null) return another == TimeSpan.Zero ? -1 : another.Value.CompareTo(TimeSpan.Zero);
            return @this.Value.CompareTo(another.Value);
        }
    }
}