namespace Olive.Entities
{
    using System;

    /// <summary>
    /// Database functions for the lambdas of a report query (<see cref="ReportQueryExtensions"/>): each
    /// provider spells them in its own SQL. They only mean something inside such a lambda, which is
    /// translated rather than run, so calling one directly throws.
    /// <para>
    /// Plain C# works too where it has an obvious SQL meaning: <c>x.Date</c> (the day), <c>x.Hour</c>,
    /// <c>x.DayOfWeek</c> (Sunday is 0), <c>Trim()</c>, <c>ToLower()</c>, <c>ToUpper()</c>, <c>Replace()</c>,
    /// <c>Contains()</c> (optionally with <see cref="StringComparison.OrdinalIgnoreCase"/>), <c>StartsWith()</c>,
    /// <c>EndsWith()</c>, string <c>+</c>, <c>??</c>, <c>?:</c>, arithmetic, and <c>list.Contains(x)</c> or
    /// <c>x.IsAnyOf(list)</c>.
    /// </para>
    /// </summary>
    public static class DbFunctions
    {
        static T NotInSql<T>(string name) =>
            throw new InvalidOperationException($"DbFunctions.{name}() can only be used in a report query's lambda, where it is translated to SQL.");

        /// <summary>The time with its minutes, seconds and fractions removed.</summary>
        public static DateTime TruncateToHour(DateTime time) => NotInSql<DateTime>(nameof(TruncateToHour));

        /// <summary>The time with its minutes, seconds and fractions removed.</summary>
        public static DateTime? TruncateToHour(DateTime? time) => NotInSql<DateTime?>(nameof(TruncateToHour));

        /// <summary>The start of the window of <paramref name="minutes"/> minutes the time falls in, counting
        /// windows from midnight 2000-01-01. The minutes must be a constant.</summary>
        public static DateTime TruncateToMinutes(DateTime time, int minutes) => NotInSql<DateTime>(nameof(TruncateToMinutes));

        /// <summary>The start of the window of <paramref name="minutes"/> minutes the time falls in.</summary>
        public static DateTime? TruncateToMinutes(DateTime? time, int minutes) => NotInSql<DateTime?>(nameof(TruncateToMinutes));

        /// <summary>The first <paramref name="length"/> characters. The length must be a constant.</summary>
        public static string Left(string text, int length) => NotInSql<string>(nameof(Left));

        /// <summary>Null where the text is empty, the text otherwise (SQL's NULLIF(text, '')).</summary>
        public static string NullIfEmpty(string text) => NotInSql<string>(nameof(NullIfEmpty));

        /// <summary>The text compared without regard to case, whatever its column's collation.</summary>
        public static string CaseInsensitive(string text) => NotInSql<string>(nameof(CaseInsensitive));

        /// <summary>SQL's LIKE: % matches any text and _ any one character, and a backslash makes the character
        /// after it literal (ESCAPE '\'). Case follows the column's collation.</summary>
        public static bool Like(string text, string pattern) => NotInSql<bool>(nameof(Like));

        /// <summary>Whether the text is <see cref="Like"/> any of the patterns.</summary>
        public static bool LikeAny(string text, System.Collections.Generic.IEnumerable<string> patterns) => NotInSql<bool>(nameof(LikeAny));
    }
}
