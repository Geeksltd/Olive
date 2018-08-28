using System;
using System.Runtime.CompilerServices;

namespace Olive
{
    partial class OliveExtensions
    {
        const int ONE_MILLION = 1000000, TWENTY = 20;

        /// <summary> Static mapping array, used by ToWordString for Units. </summary>
        static readonly string[] NumberWordsUnits = new[]
                                                            {
                                                                "zero", "one", "two", "three", "four", "five", "six",
                                                                "seven", "eight", "nine", "ten",
                                                                "eleven", "twelve", "thirteen", "fourteen", "fifteen",
                                                                "sixteen", "seventeen",
                                                                "eighteen", "nineteen"
                                                            };

        /// <summary> Static mapping array, used by ToWordString for Tens. </summary>
        static readonly string[] NumberWordsTens = new[]
                                                           {
                                                               "zero", "ten", "twenty", "thirty", "forty", "fifty",
                                                               "sixty", "seventy", "eighty",
                                                               "ninety"
                                                           };

        /// <summary>
        /// Rounds up to nearest value with the intervals specified.
        /// </summary>
        public static int RoundUpToNearest(this int value, int roundIntervals)
        {
            var difference = roundIntervals - (value % roundIntervals);
            return value + difference;
        }

        /// <summary>
        /// Rounds down to nearest value with the intervals specified.
        /// </summary>
        public static int RoundDownToNearest(this int value, int roundIntervals) => value - (value % roundIntervals);

        /// <summary>
        /// Converts this number to a short textual representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Base32Integer ToBase32(this int @this) => new Base32Integer(@this);

        public static int CompareTo(this int? @this, int? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this < 0 ? -1 : 1;
            if (@this == null) return another < 0 ? 1 : -1;
            return @this.Value.CompareTo(another.Value);
        }

        public static string ToString(this int? @this, string format) => ($"{{0:{format}}}").FormatWith(@this);

        /// <summary>
        /// To the word string.
        /// </summary>
        /// <remarks>
        /// Some awesome code from http://stackoverflow.com/questions/2729752/converting-numbers-in-to-words-c-sharp
        /// </remarks>
        public static string ToWordString(this int @this)
        {
            if (@this == 0) return "zero";

            if (@this < 0)
                return "minus " + ToWordString(Math.Abs(@this));

            var words = "";

            if ((@this / ONE_MILLION) > 0)
            {
                words += ToWordString(@this / ONE_MILLION) + " million ";
                @this %= ONE_MILLION;
            }

            if ((@this / 1000) > 0)
            {
                words += ToWordString(@this / 1000) + " thousand ";
                @this %= 1000;
            }

            if ((@this / 100) > 0)
            {
                words += ToWordString(@this / 100) + " hundred ";
                @this %= 100;
            }

            if (@this > 0)
            {
                if (words != "") words += "and ";

                if (@this < TWENTY)
                    words += NumberWordsUnits[@this];
                else
                {
                    words += NumberWordsTens[@this / 10];
                    if ((@this % 10) > 0)
                        words += "-" + NumberWordsUnits[@this % 10];
                }
            }

            return words;
        }

        /// <summary>
        /// Emits a user readable file size (including units).
        /// </summary>
        public static string ToFileSizeString(this long fileSize, string units, int round)
        {
            if ("MB".Equals(units, StringComparison.OrdinalIgnoreCase))
                return string.Format("{0:0.0} MB", Math.Round((double)fileSize / 0x100000, round));

            var suffix = new[] { "B", "KB", "MB", "GB", "TB" };
            long index = 0;

            while (fileSize > 0x400 && index < suffix.Length)
            {
                fileSize = fileSize / 0x400;
                index++;
            }

            return string.Concat(fileSize, " ", suffix[index]);
        }

        /// <summary>
        /// Gets the size text for the given number of bytes. E.g. 4.5MB or 11KB.
        /// </summary>
        public static string ToFileSizeString(this int @this, int round = 1) => ToFileSizeString((long)@this, round);

        /// <summary>
        /// Gets the size text for the given number of bytes.
        /// </summary>
        public static string ToFileSizeString(this long @this, int round = 1)
        {
            var scale = new[] { "B", "KB", "MB", "GB", "TB" };
            if (@this == 0) return "0" + scale[0];

            var sign = Math.Sign(@this);
            @this = Math.Abs(@this);

            var place = Convert.ToInt32(Math.Floor(Math.Log(@this, 1024)));
            var num = Math.Round(@this / Math.Pow(1024, place), round);

            return (sign * num) + scale[place];
            // return (Math.Sign(size) * num).ToString() + scale[place];
        }

        /// <summary>
        /// Emits a user readable file size (including units).
        /// </summary>
        public static string ToFileSizeString(this int @this, string units, int round) =>
            ToFileSizeString((long)@this, units, round);

        /// <summary>
        /// Return this value as a percentages the of the given total.
        /// </summary>
        /// <param name="multiplyBy100">Multiply this by 100.</param>
        /// <param name="roundTo">Rounding decimals to.</param>
        public static double AsPercentageOf(this int @this, int total, bool multiplyBy100 = true, int? roundTo = 0) =>
            AsPercentageOf((double)@this, total, multiplyBy100, roundTo);

        /// <summary>
        /// E.g. converts 1 to 1st. Or converts 13 to 13th.
        /// </summary>
        public static string ToOrdinal(this int @this)
        {
            switch (@this % 100)
            {
                case 11:
                case 12:
                case 13:
                    return @this + "th";
                default:
                    // Other numbers are fine.
                    break;
            }

            switch (@this % 10)
            {
                case 1:
                    return @this + "st";
                case 2:
                    return @this + "nd";
                case 3:
                    return @this + "rd";
                default:
                    return @this + "th";
            }
        }

        /// <summary>
        /// Concerts this integer value to GUID.
        /// </summary>
        public static Guid ToGuid(this int @this)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(@this).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}