namespace Olive
{
    partial class OliveExtensions
    {
        public static string ToString(this bool? value, string format) => ("{0:" + format + "}").FormatWith(value);

        /// <summary>
        /// Returns Yes or No string depending on whether the result is true of false.
        /// </summary>
        public static string ToYesNoString(this bool value, string yes = "Yes", string no = "No") => value ? yes : no;

        /// <summary>
        /// Returns Yes or No string depending on whether the result is true of false.
        /// </summary>
        public static string ToYesNoString(this bool? value, string yes = "Yes", string no = "No") =>
            value.HasValue ? ToYesNoString(value.Value) : string.Empty;

        public static int CompareTo(this bool? @this, bool? another)
        {
            if (@this == another) return 0;
            if (another == null) return @this.Value ? 1 : -1;
            if (@this == null) return another.Value ? -1 : 1;
            return @this.Value.CompareTo(another.Value);
        }
    }
}
