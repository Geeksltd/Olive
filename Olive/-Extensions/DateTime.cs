using Olive._Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Olive
{
    partial class OliveExtensions
    {
        const int WEEK_DAYS_COUNT = 7;
        const int CHRISTMAS_DAY = 25;
        const int MINUS_THREE = -3;
        const int MINUS_SIX = -6;
        const int MAC_DAY_COUNT_IN_MONTH = 31;
        const int HOURS_IN_A_DAY = 24;
        static readonly DateTime UnixEpoch = new(1970, 1, 1);

        static readonly UKHoliday ukHoliday = new UKHoliday();

        /// <summary>
        /// Determines if a specified date is an English national holiday or weekend.
        /// </summary>
        public static bool IsEnglishHoliday(this DateTime @this, bool considerWeekends = true)
        {
            @this = @this.Date; // drop time.

            bool isWeekend = ukHoliday.IsWeekend(@this);

            bool isUkHoliday = ukHoliday.IsUkHoliday(@this);

            if (considerWeekends)
            {
                return isWeekend || isUkHoliday;
            }
            else
            {
                return isUkHoliday;
            }
        }

        /// <summary>
        /// Gets the first upcoming specified week day.
        /// </summary>
        /// <param name="dayOfWeek">The number of day of week(0 to 6)</param>
        /// <param name="skipToday">If skipToday is true, method starts from tomorrow.</param>
        public static DateTime GetUpcoming(this DateTime @this, DayOfWeek dayOfWeek, bool skipToday = false)
        {
            if (skipToday) @this = @this.Date.AddDays(1);
            else @this = @this.Date;

            while (true)
            {
                if (@this.DayOfWeek == dayOfWeek) return @this;
                @this = @this.AddDays(1);
            }
        }

        /// <summary>
        /// Gets the last occurance of the specified week day.
        /// </summary>
        /// <param name="dayOfWeek">The number of day of week(0 to 6)</param>
        /// <param name="skipToday">If skipToday is true, the method does not contain Today.</param>
        public static DateTime GetLast(this DateTime @this, DayOfWeek dayOfWeek, bool skipToday = false)
        {
            @this = @this.Date;

            if (skipToday) @this = @this.AddDays(-1);

            while (true)
            {
                if (@this.DayOfWeek == dayOfWeek) return @this;

                @this = @this.AddDays(-1);
            }
        }

        /// <summary>
        /// Determines whether this value is before than TimeSpan parameter
        /// </summary>
        /// <param name="span">Is a TimeSpan which is compared with this value.</param>
        public static bool IsOlderThan(this DateTime @this, TimeSpan span) => (LocalTime.Now - @this) > span;

        /// <summary>
        /// Determines whether this value is after than TimeSpan parameter
        /// </summary>
        /// <param name="span">Is a TimeSpan which is compared with this value.</param>
        public static bool IsNewerThan(this DateTime source, TimeSpan span) => (LocalTime.Now - source) < span;

        /// <summary>
        /// Determines whether this value is equal or after than TimeSpan parameter
        /// </summary>
        /// <param name="otherDate">Is a date which is compared with this value.</param>
        public static bool IsAfterOrEqualTo(this DateTime @this, DateTime otherDate) => @this >= otherDate;

        /// <summary>
        /// Determines whether this value is equal or before than TimeSpan parameter
        /// </summary>
        /// <param name="otherDate">Is a TimeSpan which is compared with this value.</param>
        public static bool IsBeforeOrEqualTo(this DateTime @this, DateTime otherDate) => @this <= otherDate;

        /// <summary>
        /// Determines whether this day is in the same week (Monday to Sunday) as the specified other date.
        /// </summary>
        /// <param name="other">Is a date which is compared with this value.</param>
        public static bool IsInSameWeek(this DateTime day, DateTime other)
        {
            day = day.Date;

            var beginningOfWeek = day.GetBeginningOfWeek();

            if (other < beginningOfWeek) return false;
            if (other >= beginningOfWeek.AddDays(WEEK_DAYS_COUNT)) return false;

            return true;
        }

        /// <summary>
        /// Determines whether this day is in the same month as the specified other date.
        /// </summary>
        /// <param name="other">Is a date which is compared with this value.</param>
        public static bool IsInSameMonth(this DateTime @this, DateTime other) => @this.Month == other.Month && @this.Year == other.Year;

        /// <summary>
        /// Gets the number of days in this year.
        /// </summary>
        public static int DaysInYear(this DateTime @this) => new DateTime(@this.Year, 12, MAC_DAY_COUNT_IN_MONTH).DayOfYear;

        /// <summary>
        /// Gets the number of days in this month.
        /// </summary>
        public static int DaysInMonth(this DateTime @this) => DateTime.DaysInMonth(@this.Year, @this.Month);

        /// <summary>
        /// Gets the mid-night of Monday of this week.
        /// </summary>
        public static DateTime GetBeginningOfWeek(this DateTime @this) => @this.Date.GetLast(DayOfWeek.Monday, skipToday: false);

        /// <summary>
        /// Gets one tick before the start of next week.
        /// </summary>
        /// <param name="startOfWeek">The day of week which you want to set the first day of week. Default is Monday.</param>
        public static DateTime GetEndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday) =>
            date.GetUpcoming(startOfWeek, skipToday: true).AddTicks(-1);

        /// <summary>
        /// Gets the mid-night of the first day of this month.
        /// </summary>
        public static DateTime GetBeginningOfMonth(this DateTime day) => new DateTime(day.Year, day.Month, 1);

        /// <summary>
        /// Gets the end of this day (one tick before the next day).
        /// </summary>
        public static DateTime EndOfDay(this DateTime date)
        {
            try
            {
                return date.Date.AddDays(1).AddTicks(-1);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MaxValue;
            }
        }

        /// <summary>
        /// Gets the end of this day (one tick before the next day).
        /// </summary>
        public static DateTime? EndOfDay(this DateTime? @this) => @this?.EndOfDay();

        /// <summary>
        /// Determines whether this date is in the future.
        /// </summary>
        public static bool IsInTheFuture(this DateTime @this) => @this > LocalTime.Now;

        /// <summary>
        /// Determines whether this date is in the future.
        /// </summary>
        public static bool IsTodayOrFuture(this DateTime @this) => @this.Date >= LocalTime.Today;

        /// <summary>
        /// Determines whether this date is in the future.
        /// </summary>
        public static bool IsToday(this DateTime date) => date.Date == LocalTime.Today;

        /// <summary>
        /// Determines whether this date is in the past. It returns true if the the date is smaller than LocalTime.Now.
        /// </summary>
        public static bool IsInThePast(this DateTime date) => date < LocalTime.Now;

        /// <summary>
        /// Determines whether this date is greater than another one. It returns true if this date is greater.
        /// </summary>
        /// <param name="otherDate">Compared by this date</param>
        public static bool IsAfter(this DateTime @this, DateTime otherDate) => @this > otherDate;

        /// <summary>
        /// Determines whether this date is smaller than another one. It returns true if this date is smaller.
        /// </summary>
        /// <param name="otherDate">Compared by this date</param>
        public static bool IsBefore(this DateTime @this, DateTime otherDate) => @this < otherDate;

        /// <summary>
        /// It shows the time as hh:mm AM or hh:mm PM. E.g. 4am or 6:30pm.
        /// </summary>
        public static string ToSmallTime(this DateTime date) => date.ToString("h:mm").TrimEnd(":00") + date.ToString("tt").ToLower();

        /// <summary>
        /// Determines whether this date is weekend. It returns true if this date is Saturday or Sunday.
        /// </summary>
        public static bool IsWeekend(this DateTime @this) => @this.DayOfWeek == DayOfWeek.Sunday || @this.DayOfWeek == DayOfWeek.Saturday;

        /// <summary>
        /// Gets the specific working day after this date.
        /// </summary>
        /// <param name="days">Added the value of this parameter to this date</param>
        /// <param name="considerEnglishBankHolidays">determines whether English Bank Holidays are considered</param>
        /// <param name="considerWeekends">determines whether Weekends are considered</param>
        /// /// <param name="includestartdate">false by default</param>
       
        public static DateTime AddWorkingDays(this DateTime @this, int days, bool considerEnglishBankHolidays = true,  bool includeStartDate = false,bool considerWeekends = true)
        {
            if (days == 0) return @this;

            var result = @this;
            int increment = days > 0 ? 1 : -1;

            if (includeStartDate) result = result.AddDays(-increment);

            for (var day = 0; day < Math.Abs(days); day++)
            {
                result = days > 0 ?
                    result.NextWorkingDay(considerEnglishBankHolidays, considerWeekends) :
                    result.PreviousWorkingDay(considerEnglishBankHolidays, considerWeekends);
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="to">to date</param>
        /// <param name="includestartdate">true by default</param>
        /// <param name="includeenddate">true by default</param>
        /// <param name="considerEnglishBankHolidays">determines whether English Bank Holidays are considered</param>
        /// <param name="considerWeekends">determines whether Weekends are considered</param>
        /// <returns></returns>
        public static int WorkingDaysTo(this DateTime @this, DateTime to, bool includeStartDate = true, bool includeEndDate = true, bool considerEnglishBankHolidays = true, bool considerWeekends = true)
        {
            int result = 0;

            var fromDate = @this.Date;
            var _to = to.Date;

            if (!includeStartDate) fromDate = fromDate.AddDays(1);
            if (!includeEndDate) _to = _to.AddDays(-1);

            while (fromDate <= _to)
            {
                bool isWorkingDay = true;

                if (considerWeekends) isWorkingDay &= !ukHoliday.IsWeekend(fromDate);
                if (considerEnglishBankHolidays) isWorkingDay &= !ukHoliday.IsUkHoliday(fromDate);

                if (isWorkingDay) result++;

                fromDate = fromDate.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Gets the difference day and time between this date and now.
        /// </summary>
        /// <param name="precisionParts">Determines the number of output parts</param>
        /// <param name="longForm">determines whether the abstractions of Hours, Minutes or Seconds are shown</param>
        public static string ToTimeDifferenceString(this DateTime? @this, int precisionParts = 2, bool longForm = true)
        {
            if (@this == null) return null;
            return ToTimeDifferenceString(@this.Value, precisionParts, longForm);
        }

        /// <summary>
        /// Gets the difference day and time between this date and now.
        /// </summary>
        public static string ToTimeDifferenceString(this DateTime @this) => ToTimeDifferenceString(@this, longForm: true);

        /// <summary>
        /// Gets the difference day and time between this date and now.
        /// </summary>
        /// <param name="longForm">determines whether the abstractions of Hours, Minutes or Seconds are shown</param>
        public static string ToTimeDifferenceString(this DateTime @this, bool longForm) => ToTimeDifferenceString(@this, 2, longForm);

        /// <summary>
        /// Gets the difference day and time between this date and now.
        /// </summary>
        /// <param name="precisionParts">Determines the number of output parts</param>
        public static string ToTimeDifferenceString(this DateTime @this, int precisionParts) => ToTimeDifferenceString(@this, precisionParts, longForm: true);

        /// <summary>
        /// Gets the difference day and time between this date and now.
        /// </summary>
        /// <param name="precisionParts">Determines the number of output parts</param>
        /// <param name="longForm">determines whether the abstractions of Hours, Minutes or Seconds are shown</param>
        public static string ToTimeDifferenceString(this DateTime @this, int precisionParts, bool longForm)
        {
            var now = LocalTime.Now;

            if (now == @this)
            {
                if (longForm) return "Just now";
                else return "Now";
            }

            if (now > @this)
                return now.Subtract(@this).ToNaturalTime(precisionParts, longForm) + " ago";
            else
                return @this.Subtract(now).ToNaturalTime(precisionParts, longForm) + " later";
        }

        /// <summary>
        /// Gets this value as string of date.
        /// </summary>
        /// <param name="format">Set the output format</param>
        public static string ToString(this DateTime? @this, string format) => ($"{{0:{format}}}").FormatWith(@this);

        /// <summary>
        /// Gets the next working day.
        /// </summary>
        /// <param name="considerEnglishHolidays">determines whether English Bank Holidays are considered</param>
        /// <param name="considerWeekends">determines whether Weekends are considered</param>
        /// /// <param name="includestartdate">false by default</param>
        public static DateTime NextWorkingDay(this DateTime @this, bool considerEnglishHolidays = true, bool considerWeekends = true)
        {
            DateTime result = @this.AddDays(1);

            while ((considerEnglishHolidays && result.IsEnglishHoliday(considerWeekends)) || (considerWeekends && ukHoliday.IsWeekend(result)))
            {
                result = result.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Gets the days between this day and the specified other day.
        /// It will remove TIME information. 
        /// </summary>
        /// <param name="other">Determines whether English Bank Holidays are considered</param>
        /// <param name="inclusive">determines whether the result has the first and last days.</param>
        public static IEnumerable<DateTime> GetDaysInBetween(this DateTime @this, DateTime other, bool inclusive = false)
        {
            @this = @this.Date;
            other = other.Date;

            var from = @this <= other ? @this : other;
            var to = @this > other ? @this : other;

            var count = (int)to.Subtract(from).TotalDays;

            if (!inclusive) count--;
            else count++;

            if (count < 1) return new DateTime[0];

            var numbers = Enumerable.Range(inclusive ? 0 : 1, count);

            var result = numbers.Select(i => from.AddDays(i));

            if (@this > other) return result.Reverse();
            else return result;
        }

        /// <summary>
        /// Gets the previous working day.
        /// </summary>
        /// <param name="considerEnglishHolidays">determines whether English Bank Holidays are considered</param>
        /// <param name="considerWeekends">determines whether Weekends are considered</param>
        /// /// <param name="includestartdate">false by default</param>
        public static DateTime PreviousWorkingDay(this DateTime @this, bool considerEnglishHolidays = true, bool considerWeekends = true)
        {
            DateTime result = @this.AddDays(-1);

            while ((considerEnglishHolidays && result.IsEnglishHoliday(considerWeekends)) || (considerWeekends && ukHoliday.IsWeekend(result)))
            {
                result = result.AddDays(-1);
            }

            return result;
        }

        /// <summary>
        /// Gets useful and readable format of the date.
        /// </summary>
        public static string ToFriendlyDateString(this DateTime @this)
        {
            string formattedDate;

            if (@this.Date == LocalTime.Today)
                formattedDate = "Today";

            else if (@this.Date == LocalTime.Today.AddDays(-1))
                formattedDate = "Yesterday";

            else if (@this.Date > LocalTime.Today.AddDays(MINUS_SIX))
                // *** Show the Day of the week
                formattedDate = @this.ToString("dddd");

            else
                formattedDate = @this.ToString("MMMM dd, yyyy");

            // append the time portion to the output
            formattedDate += " @ " + @this.ToString("t").ToLower();
            return formattedDate;
        }

        /// <summary>
        /// Determines whether this date is between two sepcified dates.
        /// </summary>
        /// <param name="from">The date which determines the begin of period.</param>
        /// <param name="to">The date which determines the end of period</param>
        /// <param name="includingEdges">Determines whether from and to parameters included in period</param>
        public static bool IsBetween(this DateTime @this, DateTime from, DateTime to, bool includingEdges = true)
        {
            if (from > to)
                throw new ArgumentException("\"From\" date should be smaller than or equal to \"To\" date.");

            if (@this < from || @this > to) return false;

            if (!includingEdges)
                if (@this == from || @this == to) return false;

            return true;
        }

        /// <summary>
        /// Calculates the total working times in the specified duration which are between the two specified day-hours.
        /// This can be used to calculate working hours in a particular duration.
        /// </summary>
        /// <param name="period">determines the TimeSpan of the period</param>
        /// <param name="workingStartTime">determines the begin of the period</param>
        /// <param name="workingEndTime">determines the end of the period</param>
        /// <param name="considerEnglishBankHolidays">determines whether English Bank Holidays are considered</param>
        public static TimeSpan CalculateTotalWorkingHours(this DateTime @this, TimeSpan period, TimeSpan workingStartTime, TimeSpan workingEndTime, bool considerEnglishBankHolidays = true)
        {
            if (period < TimeSpan.Zero)
                throw new ArgumentException("duration should be a positive time span.");

            if (workingStartTime < TimeSpan.Zero || workingStartTime >= HOURS_IN_A_DAY.Hours())
                throw new ArgumentException("fromTime should be greater than or equal to 0, and smaller than 24.");

            if (workingEndTime <= TimeSpan.Zero || workingEndTime > HOURS_IN_A_DAY.Hours())
                throw new ArgumentException("toTime should be greater than 0, and smaller than or equal to 24.");

            var result = TimeSpan.Zero;

            // var inclusiveTimeSpan = toTime - fromTime;

            var workingTimesInday = new List<KeyValuePair<TimeSpan, TimeSpan>>();

            if (workingEndTime > workingStartTime)
            {
                workingTimesInday.Add(new KeyValuePair<TimeSpan, TimeSpan>(workingStartTime, workingEndTime));
            }
            else
            {
                workingTimesInday.Add(new KeyValuePair<TimeSpan, TimeSpan>(workingEndTime, 1.Days()));
                workingTimesInday.Add(new KeyValuePair<TimeSpan, TimeSpan>(TimeSpan.Zero, workingStartTime));
            }

            // For each working day in the range, calculate relevant times
            for (var day = @this.Date; day < @this.Add(period); day = day.AddWorkingDays(1, considerEnglishBankHolidays))
            {
                if (!ukHoliday.IsWeekend(day) && !(ukHoliday.IsUkHoliday(day) && considerEnglishBankHolidays))
                {
                    foreach (var range in workingTimesInday)
                    {
                        var from = day.Add(range.Key);

                        if (from < @this) from = @this;

                        var to = day.Add(range.Value);
                        if (to < @this) continue;

                        if (to > @this.Add(period)) to = @this.Add(period);

                        var amount = to - from;

                        if (amount < TimeSpan.Zero) continue;

                        result += amount;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the Date of the beginning of Quarter for this DateTime value (time will be 00:00:00).
        /// </summary>
        public static DateTime GetBeginningOfQuarter(this DateTime @this)
        {
            var startMonths = new[] { 1, 4, 7, 10 };

            for (var i = startMonths.Length - 1; i >= 0; i--)
            {
                var beginningOfQuarter = new DateTime(@this.Year, startMonths[i], 1);

                if (@this >= beginningOfQuarter)
                    return beginningOfQuarter;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Returns the Date of the end of Quarter for this DateTime value (time will be 11:59:59).
        /// </summary>
        public static DateTime GetEndOfQuarter(this DateTime @this) =>
            @this.GetBeginningOfQuarter().AddMonths(4).GetBeginningOfQuarter().AddTicks(-1);

        /// <summary>
        /// Returns the Date of the end of Month for this DateTime value (time will be 11:59:59).
        /// </summary>
        public static DateTime GetEndOfMonth(this DateTime date) => date.GetBeginningOfMonth().AddMonths(1).AddTicks(-1);

        /// <summary>
        /// Determines whether this date is the last day of its month or not.
        /// </summary>
        public static bool IsLastDayOfMonth(this DateTime @this) => @this.Date == @this.GetEndOfMonth().Date;

        /// <summary>
        /// Gets the last date with the specified month and day.
        /// </summary>
        /// <param name="month">The number of month which is used in the method.</param>
        /// <param name="day">The number of day which is used in the method.</param>
        public static DateTime GetLast(this DateTime @this, CalendarMonth month, int day)
        {
            @this = @this.Date; // cut time

            var thisYear = new DateTime(@this.Year, (int)month, day);

            if (@this >= thisYear) return thisYear;
            else return new DateTime(@this.Year - 1, (int)month, day);
        }

        /// <summary>
        /// Gets the next date with the specified month and day.
        /// </summary>
        /// <param name="month">The number of month which is used in the method.</param>
        /// <param name="day">The number of day which is used in the method.</param>
        public static DateTime GetNext(this DateTime @this, CalendarMonth month, int day)
        {
            @this = @this.Date; // cut time

            var thisYear = new DateTime(@this.Year, (int)month, day);

            if (@this >= thisYear) return new DateTime(@this.Year + 1, (int)month, day);
            else return thisYear;
        }

        /// <summary>
        /// Returns the Date of the end of Year for this DateTime value (time will be 11:59:59).
        /// </summary>
        public static DateTime GetEndOfYear(this DateTime @this) => new DateTime(@this.Year + 1, 1, 1).AddTicks(-1);

        /// <summary>
        /// Gets the minimum value between this date and a specified other date.
        /// </summary>
        /// <param name="other">Is the date which is compared with this value.</param>
        public static DateTime Min(this DateTime @this, DateTime other)
        {
            if (other < @this) return other;
            else return @this;
        }

        /// <summary>
        /// Gets the maximum value between this date and a specified other date.
        /// </summary>
        /// <param name="other">Is the date which is compared with this value.</param>
        public static DateTime Max(this DateTime @this, DateTime other)
        {
            if (other > @this) return other;
            else return @this;
        }

        /// <summary>
        /// Adds the specified number of weeks and returns the result.
        /// </summary>
        /// <param name="numberOfWeeks">the specified number of weeks</param>
        public static DateTime AddWeeks(this DateTime @this, int numberOfWeeks) => @this.AddDays(WEEK_DAYS_COUNT * numberOfWeeks);

        /// <summary>
        /// Gets the latest date with the specified day of week and time that is before (or same as) this date.
        /// </summary>
        /// <param name="day">the specified number of weeks</param>
        /// <param name="timeOfDay">the specified number of weeks</param>
        public static DateTime GetLast(this DateTime @this, DayOfWeek day, TimeSpan timeOfDay)
        {
            var result = @this.GetLast(day).Add(timeOfDay);

            if (result > @this) return result.AddWeeks(-1);
            else return result;
        }

        /// <summary>
        /// Returns the local time equivalent of this UTC date value based on the TimeZone specified in Localtime.TimeZoneProvider.
        /// Use this instead of ToLocalTime() so you get control over the TimeZone.
        /// </summary>
        public static DateTime? ToLocal(this DateTime? @this) => @this?.ToLocal();

        /// <summary>
        /// Returns the local time equivalent of this UTC date value based on the TimeZone specified in Localtime.CurrentTimeZone().
        /// Use this instead of ToLocalTime() so you get control over the TimeZone.
        /// </summary>
        public static DateTime ToLocal(this DateTime @this) => @this.ToLocal(LocalTime.CurrentTimeZone());

        /// <summary>
        /// Returns the local time equivalent of this UTC date value based on the TimeZone specified in Localtime.CurrentTimeZone().
        /// </summary>
        /// <param name="timeZone">the specified TimeZone</param>
        public static DateTime ToLocal(this DateTime @this, TimeZoneInfo timeZone) =>
            new DateTime(@this.Ticks, DateTimeKind.Local).Add(timeZone.GetUtcOffset(@this));

        /// <summary>
        /// Returns the equivalent Universal Time (UTC) of this local date value.
        /// </summary>
        public static DateTime? ToUniversal(this DateTime? @this) => @this?.ToUniversal();

        /// <summary>
        /// Returns the equivalent Universal Time (UTC) of this local date value.
        /// </summary>
        public static DateTime ToUniversal(this DateTime @this) =>
            @this.ToUniversal(sourceTimezone: LocalTime.CurrentTimeZone());

        /// <summary>
        /// Returns the equivalent Universal Time (UTC) of this local date value.
        /// </summary>
        /// <param name="sourceTimezone">the specified TimeZone</param>
        public static DateTime ToUniversal(this DateTime @this, TimeZoneInfo sourceTimezone) =>
            new DateTime(@this.Ticks, DateTimeKind.Utc).Subtract(sourceTimezone.BaseUtcOffset);

        /// <summary>
        ///  Rounds this up to the nearest whole second.
        /// </summary>
        public static DateTime RoundToSecond(this DateTime @this) => @this.Round(1.Seconds());

        /// <summary>
        ///  Rounds this up to the nearest whole minute.
        /// </summary>
        public static DateTime RoundToMinute(this DateTime @this) => @this.Round(1.Minutes());

        /// <summary>
        ///  Rounds this up to the nearest whole hour.
        /// </summary>
        public static DateTime RoundToHour(this DateTime @this) => @this.Round(1.Hours());

        /// <summary>
        ///  Rounds this up to the nearest interval (e.g. second, minute, hour, etc).
        /// </summary>
        /// <param name="nearest">Nearest interval or rounding (e.g. second, minute, hour, etc).</param>
        public static DateTime Round(this DateTime @this, TimeSpan nearest)
        {
            if (nearest == TimeSpan.Zero) return @this;

            var remainder = @this.Ticks % nearest.Ticks;

            var result = @this.AddTicks(-remainder);

            if (remainder >= nearest.Ticks / 2) result = result.Add(nearest);

            return result;
        }

        /// <summary>
        ///  Compare this value with another date parameter.
        /// </summary>
        /// <param name="another">The date which is compared with this value.</param>
        public static int CompareTo(this DateTime? @this, DateTime? another)
        {
            if (@this == another) return 0;
            if (another == null) return 1;
            if (@this == null) return -1;
            return @this.Value.CompareTo(another.Value);
        }

        /// <summary>
        /// Gets the total number of seconds elapsed since 1st Jan 1970.
        /// </summary>
        public static long ToUnixTime(this DateTime @this)
        {
            if (@this.Kind != DateTimeKind.Utc)
                @this = @this.ToUniversal();
            return (int)@this.Subtract(UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Gets the local date time based on the provided seconds elapsed since 1st Jan 1970.
        /// </summary>
        public static DateTime FromUnixTime(this long @this)
            => UnixEpoch.AddSeconds(@this).ToLocal();

        public static int GetWeekOfYear(this DateTime time)
            => time.GetWeekOfYear(CultureInfo.InvariantCulture.Calendar);

        public static int GetWeekOfYear(this DateTime time, Calendar calendar)
        {
            var day = time.GetDayOfWeek(calendar);

            if (day is >= DayOfWeek.Monday and <= DayOfWeek.Wednesday)
                time = time.AddDays(3);

            return calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static int GetDayOfYear(this DateTime time)
            => time.GetDayOfYear(CultureInfo.InvariantCulture.Calendar);

        public static int GetDayOfYear(this DateTime time, Calendar calendar)
            => calendar.GetDayOfYear(time);

        public static int GetDayOfMonth(this DateTime time)
            => time.GetDayOfMonth(CultureInfo.InvariantCulture.Calendar);

        public static int GetDayOfMonth(this DateTime time, Calendar calendar)
            => calendar.GetDayOfMonth(time);

        public static DayOfWeek GetDayOfWeek(this DateTime time)
            => time.GetDayOfWeek(CultureInfo.InvariantCulture.Calendar);

        public static DayOfWeek GetDayOfWeek(this DateTime time, Calendar calendar)
            => calendar.GetDayOfWeek(time);
    }

    public enum CalendarMonth
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}