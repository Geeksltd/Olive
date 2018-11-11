namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Converts the value of a boolean object to its equivalent string representation for the specified custom text instead of the default "True" or "False".
        /// </summary>
        /// <param name="format">The output format string</param>
        public static string ToString(this bool? value, string format) => ("{0:" + format + "}").FormatWith(value);

        /// <summary>
        /// Returns Yes or No depending on whether the result is true of false.
        /// </summary>
        /// <param name="yes">The output string if this value is True. the default value is Yes.</param>
        /// <param name="no">The output string if this value is False.. the default value is No.</param>
        public static string ToYesNoString(this bool value, string yes = "Yes", string no = "No") => value ? yes : no;

        /// <summary>
        /// Returns Yes or No depending on whether the result is true of false.
        /// </summary>
        /// <param name="yes">The output string if this value is True. the default value is Yes.</param>
        /// <param name="no">The output string if this value is False.. the default value is No.</param>
        public static string ToYesNoString(this bool? value, string yes = "Yes", string no = "No") =>
            value.HasValue ? ToYesNoString(value.Value) : string.Empty;

        /// <summary>
        /// Compares two Boolean object and returns 0 if both are equal.
        /// </summary>
        /// <param name="another">It compared with this value.</param>
        public static int CompareTo(this bool? @this, bool? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this.Value ? 1 : -1;
            if (@this == null) return another.Value ? -1 : 1;
            return @this.Value.CompareTo(another.Value);
        }
    }
}
