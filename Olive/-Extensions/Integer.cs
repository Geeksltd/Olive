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
        public static Base32Integer ToBase32(this int value) => new Base32Integer(value);

        public static int CompareTo(this int? @this, int? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this < 0 ? -1 : 1;
            if (@this == null) return another < 0 ? 1 : -1;
            return @this.Value.CompareTo(another.Value);
        }

        public static string ToString(this int? value, string format) => ($"{{0:{format}}}").FormatWith(value);

        /// <summary>
        /// To the word string.
        /// </summary>
        /// <remarks>
        /// Some awesome code from http://stackoverflow.com/questions/2729752/converting-numbers-in-to-words-c-sharp
        /// </remarks>
        public static string ToWordString(this int number)
        {
            if (number == 0) return "zero";

            if (number < 0)
                return "minus " + ToWordString(Math.Abs(number));

            var words = "";

            if ((number / ONE_MILLION) > 0)
            {
                words += ToWordString(number / ONE_MILLION) + " million ";
                number %= ONE_MILLION;
            }

            if ((number / 1000) > 0)
            {
                words += ToWordString(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ToWordString(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "") words += "and ";

                if (number < TWENTY)
                    words += NumberWordsUnits[number];
                else
                {
                    words += NumberWordsTens[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + NumberWordsUnits[number % 10];
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
        public static string ToFileSizeString(this int size, int round = 1) => ToFileSizeString((long)size, round);

        /// <summary>
        /// Gets the size text for the given number of bytes.
        /// </summary>
        public static string ToFileSizeString(this long size, int round = 1)
        {
            var scale = new[] { "B", "KB", "MB", "GB", "TB" };
            if (size == 0) return "0" + scale[0];

            var sign = Math.Sign(size);
            size = Math.Abs(size);

            var place = Convert.ToInt32(Math.Floor(Math.Log(size, 1024)));
            var num = Math.Round(size / Math.Pow(1024, place), round);

            return (sign * num) + scale[place];
            // return (Math.Sign(size) * num).ToString() + scale[place];
        }

        /// <summary>
        /// Emits a user readable file size (including units).
        /// </summary>
        public static string ToFileSizeString(this int fileSize, string units, int round) =>
            ToFileSizeString((long)fileSize, units, round);

        /// <summary>
        /// Return this value as a percentages the of the given total.
        /// </summary>
        /// <param name="multiplyBy100">Multiply this by 100.</param>
        /// <param name="roundTo">Rounding decimals to.</param>
        public static double AsPercentageOf(this int value, int total, bool multiplyBy100 = true, int? roundTo = 0) =>
            AsPercentageOf((double)value, total, multiplyBy100, roundTo);

        /// <summary>
        /// E.g. converts 1 to 1st. Or converts 13 to 13th.
        /// </summary>
        public static string ToOrdinal(this int number)
        {
            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
                default:
                    // Other numbers are fine.
                    break;
            }

            switch (number % 10)
            {
                case 1:
                    return number + "st";
                case 2:
                    return number + "nd";
                case 3:
                    return number + "rd";
                default:
                    return number + "th";
            }
        }

        /// <summary>
        /// Concerts this integer value to GUID.
        /// </summary>
        public static Guid ToGuid(this int value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}