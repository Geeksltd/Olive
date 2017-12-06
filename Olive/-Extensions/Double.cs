namespace Olive
{
    partial class OliveExtensions
    {
        const int HALF_CIRCLE_DEGREES = 180;

        /// <summary>
        /// Rounds this value.
        /// </summary>
        public static double Round(this double value, int digits) =>
            (double)Math.Round((decimal)value, digits, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Rounds this value.
        /// </summary>
        public static decimal Round(this decimal value, int digits) =>
            Math.Round(value, digits, MidpointRounding.AwayFromZero);

        /// <summary>
        /// In mathematics and computer science, truncation is the term for limiting the number of digits right of the decimal point, by discarding the least significant ones.
        /// Note that in some cases, truncating would yield the same result as rounding, but truncation does not round up or round down the digits; it merely cuts off at the specified digit.
        /// </summary>
        public static double Truncate(this double value, int places)
        {
            var multiplier = Math.Pow(10, (double)places);

            if (value > 0)
                return Math.Floor(value * multiplier) / multiplier;
            else
                return Math.Ceiling(value * multiplier) / multiplier;
        }

        public static string ToString(this double? value, string format) => ($"{{0:{format}}}").FormatWith(value);

        public static string ToString(this decimal? value, string format) => ($"{{0:{format}}}").FormatWith(value);

        /// <summary>
        /// Drops the floating point digits from the end of the money string.
        /// For example for 1500.00 it will yield "£1,500" and for 18.56 it will yield "£18.56".
        /// </summary>
        public static string ToShortMoneyString(this double value) => "{0:c}".FormatWith(value).TrimEnd(".00");

        public static string ToShortMoneyString(this double? value)
        {
            if (value.HasValue) return value.ToShortMoneyString();
            else return string.Empty;
        }

        /// <summary>
        /// Drops the floating point digits from the end of the money string.
        /// For example for 1500.00 it will yield "£1,500" and for 18.56 it will yield "£18.56".
        /// </summary>
        public static string ToInformalMoneyString(this double value)
        {
            var identifiers = new[] { "k", "m", "bn" };

            for (var i = identifiers.Length; i > 0; i--)
            {
                var power = 2 + 3 * (i - 1);

                var figure = Math.Pow(10, power);

                if (value % figure == 0 && (value / figure) > 10)
                {
                    value = value / figure * 10;

                    if (value > 1000)
                        return value.ToShortMoneyString() + identifiers[i - 1];
                    else
                        return value.ToShortMoneyString()[0].ToString() + value + identifiers[i - 1];
                }
            }

            return value.ToShortMoneyString();
        }

        /// <summary>
        /// Converts degree into radians.
        /// </summary>
        public static double ToRadians(this double degrees) => Math.PI * degrees / HALF_CIRCLE_DEGREES;

        /// <summary>
        /// Return this value as a percentages the of the given total.
        /// </summary>       
        public static double AsPercentageOf(this double value, double total, bool multiplyBy100 = true, int? roundTo = null)
        {
            var pc = value / total;

            if (double.IsNaN(pc) || double.IsInfinity(pc)) return 0d;

            if (multiplyBy100) pc = pc * 100d;

            if (roundTo.HasValue) pc = pc.Round(roundTo.Value);

            return pc;
        }

        /// <summary>
        /// Return this value as a percentages the of the given total.
        /// </summary>
        public static decimal AsPercentageOf(this decimal value, decimal total, bool multiplyBy100 = true, int? roundTo = null)
        {
            var pc = value / total;

            if (multiplyBy100) pc = pc * 100;

            if (roundTo.HasValue)
                pc = Math.Round(pc, roundTo.Value);

            return pc;
        }

        /// <summary>
        /// Rounds up to nearest value.
        /// </summary>
        public static double RoundUpToNearest(this double value, double roundIntervals)
        {
            var remainder = value % roundIntervals;
            if (remainder == 0) return value;

            return value + (roundIntervals - remainder);
        }

        /// <summary>
        /// Rounds up to nearest value.
        /// </summary>
        public static decimal RoundUpToNearest(this decimal value, decimal roundIntervals)
        {
            var remainder = value % roundIntervals;
            if (remainder == 0) return value;

            return value + (roundIntervals - remainder);
        }

        /// <summary>
        /// Rounds down to nearest value with the intervals specified.
        /// </summary>
        public static double RoundDownToNearest(this double value, double roundIntervals) => value - (value % roundIntervals);

        /// <summary>
        /// Rounds down to nearest value with the intervals specified.
        /// </summary>
        public static decimal RoundDownToNearest(this decimal value, decimal roundIntervals) => value - (value % roundIntervals);

        /// <summary>
        /// Determines if this double value is almost equal to the specified other value.
        /// This should be used instead of == or != operators due to the nature of double processing in .NET.
        /// </summary>
        /// <param name="tolerance">Specifies the tolerated level of difference.</param>
        public static bool AlmostEquals(this double value, double otherValue, double tolerance = 0.00001) =>
            Math.Abs(value - otherValue) <= tolerance;

        /// <summary>
        /// Determines if this float value is almost equal to the specified other value.
        /// This should be used instead of == or != operators due to the nature of float processing in .NET.
        /// </summary>
        /// <param name="tolerance">Specifies the tolerated level of difference.</param>
        public static bool AlmostEquals(this float value, float otherValue, float tolerance = 0.001f) =>
            Math.Abs(value - otherValue) <= tolerance;

        public static float LimitMax(this float value, float maxValue) => value > maxValue ? maxValue : value;

        public static float LimitMin(this float value, float minValue) => value < minValue ? minValue : value;

        public static float LimitWithin(this float value, float minValue, float maxValue) => value.LimitMin(minValue).LimitMax(maxValue);

        public static double LimitMax(this double value, double maxValue) => value > maxValue ? maxValue : value;

        public static double LimitMin(this double value, double minValue) => value < minValue ? minValue : value;

        public static double LimitWithin(this double value, double minValue, double maxValue) => value.LimitMin(minValue).LimitMax(maxValue);

        public static int LimitMax(this int value, int maxValue) => value > maxValue ? maxValue : value;

        public static int LimitMin(this int value, int minValue) => value < minValue ? minValue : value;

        public static int LimitWithin(this int value, int minValue, int maxValue) => value.LimitMin(minValue).LimitMax(maxValue);

        public static int CompareTo(this double? @this, double? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this < 0 ? -1 : 1;
            if (@this == null) return another < 0 ? 1 : -1;
            return @this.Value.CompareTo(another.Value);
        }

        public static int CompareTo(this float? @this, float? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this < 0 ? -1 : 1;
            if (@this == null) return another < 0 ? 1 : -1;
            return @this.Value.CompareTo(another.Value);
        }

        public static int CompareTo(this decimal? @this, decimal? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this < 0 ? -1 : 1;
            if (@this == null) return another < 0 ? 1 : -1;
            return @this.Value.CompareTo(another.Value);
        }
    }
}
