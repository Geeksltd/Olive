using System;
using System.Collections.Generic;
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

        #region EasterMondays

        static DateTime[] EasterMondays = new[]{
new DateTime(1950,04,10),
new DateTime(1951,03,26),
new DateTime(1952,04,14),
new DateTime(1953,04,06),
new DateTime(1954,04,19),
new DateTime(1955,04,11),
new DateTime(1956,04,02),
new DateTime(1957,04,22),
new DateTime(1958,04,07),
new DateTime(1959,03,30),
new DateTime(1960,04,18),
new DateTime(1961,04,03),
new DateTime(1962,04,23),
new DateTime(1963,04,15),
new DateTime(1964,03,30),
new DateTime(1965,04,19),
new DateTime(1966,04,11),
new DateTime(1967,03,27),
new DateTime(1968,04,15),
new DateTime(1969,04,07),
new DateTime(1970,03,30),
new DateTime(1971,04,12),
new DateTime(1972,04,03),
new DateTime(1973,04,23),
new DateTime(1974,04,15),
new DateTime(1975,03,31),
new DateTime(1976,04,19),
new DateTime(1977,04,11),
new DateTime(1978,03,27),
new DateTime(1979,04,16),
new DateTime(1980,04,07),
new DateTime(1981,04,20),
new DateTime(1982,04,12),
new DateTime(1983,04,04),
new DateTime(1984,04,23),
new DateTime(1985,04,08),
new DateTime(1986,03,31),
new DateTime(1987,04,20),
new DateTime(1988,04,04),
new DateTime(1989,03,27),
new DateTime(1990,04,16),
new DateTime(1991,04,01),
new DateTime(1992,04,20),
new DateTime(1993,04,12),
new DateTime(1994,04,04),
new DateTime(1995,04,17),
new DateTime(1996,04,08),
new DateTime(1997,03,31),
new DateTime(1998,04,13),
new DateTime(1999,04,05),
new DateTime(2000,04,24),
new DateTime(2001,04,16),
new DateTime(2002,04,01),
new DateTime(2003,04,21),
new DateTime(2004,04,12),
new DateTime(2005,03,28),
new DateTime(2006,04,17),
new DateTime(2007,04,09),
new DateTime(2008,03,24),
new DateTime(2009,04,13),
new DateTime(2010,04,05),
new DateTime(2011,04,25),
new DateTime(2012,04,09),
new DateTime(2013,04,01),
new DateTime(2014,04,21),
new DateTime(2015,04,06),
new DateTime(2016,03,28),
new DateTime(2017,04,17),
new DateTime(2018,04,02),
new DateTime(2019,04,22),
new DateTime(2020,04,13),
new DateTime(2021,04,05),
new DateTime(2022,04,18),
new DateTime(2023,04,10),
new DateTime(2024,04,01),
new DateTime(2025,04,21),
new DateTime(2026,04,06),
new DateTime(2027,03,29),
new DateTime(2028,04,17),
new DateTime(2029,04,02),
new DateTime(2030,04,22),
new DateTime(2031,04,14),
new DateTime(2032,03,29),
new DateTime(2033,04,18),
new DateTime(2034,04,10),
new DateTime(2035,03,26),
new DateTime(2036,04,14),
new DateTime(2037,04,06),
new DateTime(2038,04,26),
new DateTime(2039,04,11),
new DateTime(2040,04,02),
new DateTime(2041,04,22),
new DateTime(2042,04,07),
new DateTime(2043,03,30),
new DateTime(2044,04,18),
new DateTime(2045,04,10),
new DateTime(2046,03,26),
new DateTime(2047,04,15),
new DateTime(2048,04,06),
new DateTime(2049,04,19),
new DateTime(2050,04,11),
new DateTime(2051,04,03),
new DateTime(2052,04,22),
new DateTime(2053,04,07),
new DateTime(2054,03,30),
new DateTime(2055,04,19),
new DateTime(2056,04,03),
new DateTime(2057,04,23),
new DateTime(2058,04,15),
new DateTime(2059,03,31),
new DateTime(2060,04,19),
new DateTime(2061,04,11),
new DateTime(2062,03,27),
new DateTime(2063,04,16),
new DateTime(2064,04,07),
new DateTime(2065,03,30),
new DateTime(2066,04,12),
new DateTime(2067,04,04),
new DateTime(2068,04,23),
new DateTime(2069,04,15),
new DateTime(2070,03,31),
new DateTime(2071,04,20),
new DateTime(2072,04,11),
new DateTime(2073,03,27),
new DateTime(2074,04,16),
new DateTime(2075,04,08),
new DateTime(2076,04,20),
new DateTime(2077,04,12),
new DateTime(2078,04,04),
new DateTime(2079,04,24),
new DateTime(2080,04,08),
new DateTime(2081,03,31),
new DateTime(2082,04,20),
new DateTime(2083,04,05),
new DateTime(2084,03,27),
new DateTime(2085,04,16),
new DateTime(2086,04,01),
new DateTime(2087,04,21),
new DateTime(2088,04,12),
new DateTime(2089,04,04),
new DateTime(2090,04,17),
new DateTime(2091,04,09),
new DateTime(2092,03,31),
new DateTime(2093,04,13),
new DateTime(2094,04,05),
new DateTime(2095,04,25),
new DateTime(2096,04,16),
new DateTime(2097,04,01),
new DateTime(2098,04,21),
new DateTime(2099,04,13)};

        #endregion EasterMondays

        /// <summary>
        /// Dictionary that contains exceptional dates for Early May Bank Holiday.
        /// Key: Year, Value: Exceptional Date for that year.
        /// </summary>
        static Dictionary<int, DateTime> EarlyMayExceptions = new Dictionary<int, DateTime>();

        #region SpringBankHolidayExceptions

        /// <summary>
        /// Dictionary that contains exceptional dates for Spring Bank Holiday.
        /// Key: Year, Value: Exceptional Date for that year.
        /// </summary>
        static Dictionary<int, DateTime> SpringBankHolidayExceptions = new Dictionary<int, DateTime>
        {
            {2012,new DateTime(2012,06,04)}
        };

        #endregion SpringBankHolidayExceptions

        #region LateSummerBankHolidayExceptions

        /// <summary>
        /// Dictionary that contains exceptional dates for Last Summer Bank Holiday.
        /// Key: Year, Value: Exceptional Date for that year.
        /// </summary>
        static Dictionary<int, DateTime> LateSummerBankHolidayExceptions = new Dictionary<int, DateTime>();

        #endregion LateSummerBankHolidayExceptions

        #region AdditionalBankHolidays

        /// <summary>
        /// Dictionary that contains exceptional dates for Last Summer Bank Holiday.
        /// Key: Year, Value: Exceptional Date for that year.
        /// </summary>
        static Dictionary<int, DateTime[]> AdditionalBankHolidays = new Dictionary<int, DateTime[]>
        {
            {2012,new[]{new DateTime(2012,6,5)}}
        };

        #endregion AdditionalBankHolidays

        /// <summary>
        /// Determines if a specified date is an English national holiday or weekend.
        /// </summary>
        public static bool IsEnglishHoliday(this DateTime date)
        {
            date = date.Date; // drop time.

            if (date.IsWeekend()) return true;

            // 1 January - New Year's Day
            if (date == GetActualHolidayDate(new DateTime(date.Year, 1, 1)))
                return true;

            // 1st Monday in May	Early May Bank Holiday
            if (date == GetEarlyMayBankHoliday(date.Year))
                return true;

            // Last Monday in May	Spring Bank Holiday
            if (date == GetSpringBankHoliday(date.Year))
                return true;

            // Last Monday in August	Late Summer Bank Holiday
            if (date == GetLateSummerBankHoliday(date.Year))
                return true;

            // December 25	Christmas Day
            if (date == GetActualHolidayDate(new DateTime(date.Year, 12, CHRISTMAS_DAY)))
                return true;

            // December 26	Boxing Day
            if (date == GetBoxingDay(date.Year))
                return true;

            try
            {
                var easterMonday = GetEasterMonday(date.Year);

                // Easter Monday
                if (date == easterMonday)
                    return true;

                // Good Friday
                if (date == easterMonday.AddDays(MINUS_THREE))
                    return true;
            }
            catch { /* No logging needed. out of supported range*/ }

            // Additional Holidays
            if (IsAdditionalBankHoliday(date))
                return true;

            return false;
        }

        /// <summary>
        /// Check if Date it is Additional bank holiday in that year
        /// </summary>
        /// <param name="date">the date to check</param>
        static bool IsAdditionalBankHoliday(DateTime date)
        {
            if (AdditionalBankHolidays.ContainsKey(date.Year))
                return AdditionalBankHolidays[date.Year].Contains<DateTime>(date);

            return false;
        }

        /// <summary>
        /// Gets the first upcoming specified week day.
        /// </summary>
        public static DateTime GetUpcoming(this DateTime date, DayOfWeek dayOfWeek, bool skipToday = false)
        {
            if (skipToday) date = date.Date.AddDays(1);
            else date = date.Date;

            while (true)
            {
                if (date.DayOfWeek == dayOfWeek) return date;

                date = date.AddDays(1);
            }
        }

        /// <summary>
        /// Gets the last occurance of the specified week day.
        /// </summary>
        public static DateTime GetLast(this DateTime date, DayOfWeek dayOfWeek, bool skipToday = false)
        {
            date = date.Date;

            if (skipToday) date = date.AddDays(-1);

            while (true)
            {
                if (date.DayOfWeek == dayOfWeek) return date;

                date = date.AddDays(-1);
            }
        }

        /// <summary>
        /// Get Early May Bank Holiday Date for the required year
        /// </summary>
        /// <param name="year">the year to check if in that year there is an exception to the normal bank holiday rule</param>
        static DateTime GetEarlyMayBankHoliday(int year)
        {
            if (EarlyMayExceptions.ContainsKey(year))
            {
                return EarlyMayExceptions[year];
            }
            else
            {
                return GetFirstWeekdayIn(year, 5 /* May */, DayOfWeek.Monday);
            }
        }

        /// <summary>
        /// Get Spring Bank Holiday Date for the required year
        /// </summary>
        /// <param name="year">the year to check if in that year there is an exception to the normal bank holiday rule</param>
        static DateTime GetSpringBankHoliday(int year)
        {
            if (SpringBankHolidayExceptions.ContainsKey(year))
                return SpringBankHolidayExceptions[year];
            else
                return GetLastWeekdayIn(year, 5, DayOfWeek.Monday);
        }

        /// <summary>
        /// Get Late Summer Bank Holiday Date for the required year
        /// </summary>
        /// <param name="year">the year to check if in that year there is an exception to the normal bank holiday rule</param>
        static DateTime GetLateSummerBankHoliday(int year)
        {
            if (LateSummerBankHolidayExceptions.ContainsKey(year))
                return LateSummerBankHolidayExceptions[year];
            else
                return GetLastWeekdayIn(year, (int)CalendarMonth.August, DayOfWeek.Monday);
        }

        static DateTime GetEasterMonday(int year)
        {
            var result = EasterMondays.FirstOrDefault(d => d.Year == year);

            if (result == DateTime.MinValue)
                throw new ArgumentException("GetEasterMonday() is not supported for the year: " + year);

            return result;
        }

        /// <summary>
        /// Get Boxing Day Holiday Date for the required year
        /// </summary>
        /// <param name="year">the year to check if in that year there is an exception to the normal bank holiday rule</param>
        static DateTime GetBoxingDay(int year)
        {
            var christmasDay = new DateTime(year, 12, CHRISTMAS_DAY);
            var result = GetActualHolidayDate(new DateTime(year, 12, CHRISTMAS_DAY + 1));

            if (christmasDay.IsWeekend())
                result = result.AddDays(1);

            return result;
        }

        static DateTime GetActualHolidayDate(DateTime originalDay)
        {
            var result = originalDay;
            while (result.IsWeekend())
                result = result.AddDays(1);

            return result;
        }

        static DateTime GetFirstWeekdayIn(int year, int month, DayOfWeek weekday)
        {
            for (var day = new DateTime(year, month, 1); ; day = day.AddDays(1))
                if (day.DayOfWeek == weekday) return day;
        }

        static DateTime GetLastWeekdayIn(int year, int month, DayOfWeek weekday)
        {
            for (var day = new DateTime(year, month, 1).AddMonths(1).AddDays(-1); ; day = day.AddDays(-1))
                if (day.DayOfWeek == weekday) return day;
        }

        public static bool IsOlderThan(this DateTime source, TimeSpan span) => (LocalTime.Now - source) > span;

        public static bool IsNewerThan(this DateTime source, TimeSpan span) => (LocalTime.Now - source) < span;

        public static bool IsAfterOrEqualTo(this DateTime date, DateTime otherDate) => date >= otherDate;

        public static bool IsBeforeOrEqualTo(this DateTime date, DateTime otherDate) => date <= otherDate;

        /// <summary>
        /// Determines whether this day is in the same week (Monday to Sunday) as the specified other date.
        /// </summary>
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
        public static bool IsInSameMonth(this DateTime day, DateTime other) => day.Month == other.Month && day.Year == other.Year;

        /// <summary>
        /// Gets the number of days in this year.
        /// </summary>
        public static int DaysInYear(this DateTime date) => new DateTime(date.Year, 12, MAC_DAY_COUNT_IN_MONTH).DayOfYear;

        /// <summary>
        /// Gets the number of days in this month.
        /// </summary>
        public static int DaysInMonth(this DateTime date) => DateTime.DaysInMonth(date.Year, date.Month);

        /// <summary>
        /// Gets the mid-night of Monday of this week.
        /// </summary>
        public static DateTime GetBeginningOfWeek(this DateTime day) => day.Date.GetLast(DayOfWeek.Monday, skipToday: false);

        /// <summary>
        /// Gets one tick before the start of next week.
        /// </summary>
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

        public static DateTime? EndOfDay(this DateTime? date) => date?.EndOfDay();

        /// <summary>
        /// Determines whether this date is in the future.
        /// </summary>
        public static bool IsInTheFuture(this DateTime date) => date > LocalTime.Now;

        /// <summary>
        /// Determines whether this date is in the future.
        /// </summary>
        public static bool IsTodayOrFuture(this DateTime date) => date.Date >= LocalTime.Today;

        /// <summary>
        /// Determines whether this date is in the future.
        /// </summary>
        public static bool IsToday(this DateTime date) => date.Date == LocalTime.Today;

        public static bool IsInThePast(this DateTime date) => date < LocalTime.Now;

        public static bool IsAfter(this DateTime date, DateTime otherDate) => date > otherDate;

        public static bool IsBefore(this DateTime date, DateTime otherDate) => date < otherDate;

        /// <summary>
        /// E.g. 4am or 6:30pm.
        /// </summary>
        public static string ToSmallTime(this DateTime date) => date.ToString("h:mm").TrimEnd(":00") + date.ToString("tt").ToLower();

        public static bool IsWeekend(this DateTime value) => value.DayOfWeek == DayOfWeek.Sunday || value.DayOfWeek == DayOfWeek.Saturday;

        public static DateTime AddWorkingDays(this DateTime date, int days, bool considerEnglishBankHolidays = true)
        {
            if (days == 0) return date;

            var result = date;

            if (days > 0)
                for (int day = 0; day < days; day++)
                    result = result.NextWorkingDay(considerEnglishBankHolidays);
            else
                for (int day = 0; day < -days; day++)
                    result = result.PreviousWorkingDay(considerEnglishBankHolidays);

            return result;
        }

        public static string ToTimeDifferenceString(this DateTime? date, int precisionParts = 2, bool longForm = true)
        {
            if (date == null) return null;
            return ToTimeDifferenceString(date.Value, precisionParts, longForm);
        }

        public static string ToTimeDifferenceString(this DateTime date) => ToTimeDifferenceString(date, longForm: true);

        public static string ToTimeDifferenceString(this DateTime date, bool longForm) => ToTimeDifferenceString(date, 2, longForm);

        public static string ToTimeDifferenceString(this DateTime date, int precisionParts) => ToTimeDifferenceString(date, precisionParts, longForm: true);

        public static string ToTimeDifferenceString(this DateTime date, int precisionParts, bool longForm)
        {
            var now = LocalTime.Now;

            if (now == date)
            {
                if (longForm) return "Just now";
                else return "Now";
            }

            if (now > date)
                return now.Subtract(date).ToNaturalTime(precisionParts, longForm) + " ago";
            else
                return date.Subtract(now).ToNaturalTime(precisionParts, longForm) + " later";
        }

        public static string ToString(this DateTime? value, string format) => ($"{{0:{format}}}").FormatWith(value);

        /// <summary>
        /// Gets the next working day.
        /// </summary>
        public static DateTime NextWorkingDay(this DateTime date, bool considerEnglishHolidays = true)
        {
            var result = date.AddDays(1);

            if (considerEnglishHolidays)
                while (result.IsEnglishHoliday())
                    result = result.AddDays(1);
            else
                while (result.IsWeekend())
                    result = result.AddDays(1);

            return result;
        }

        /// <summary>
        /// Gets the days between this day and the specified other day.
        /// It will remove TIME information. 
        /// </summary>
        public static IEnumerable<DateTime> GetDaysInBetween(this DateTime day, DateTime other, bool inclusive = false)
        {
            day = day.Date;
            other = other.Date;

            var from = day <= other ? day : other;
            var to = day > other ? day : other;

            var count = (int)to.Subtract(from).TotalDays;

            if (!inclusive) count--;
            else count++;

            if (count < 1) return new DateTime[0];

            var numbers = Enumerable.Range(inclusive ? 0 : 1, count);

            var result = numbers.Select(i => from.AddDays(i));

            if (day > other) return result.Reverse();
            else return result;
        }

        /// <summary>
        /// Gets the previous working day.
        /// </summary>
        public static DateTime PreviousWorkingDay(this DateTime date, bool considerEnglishHolidays = true)
        {
            var result = date.AddDays(-1);

            if (considerEnglishHolidays)
                while (result.IsEnglishHoliday())
                    result = result.AddDays(-1);
            else
                while (result.IsWeekend())
                    result = result.AddDays(-1);

            return result;
        }

        public static string ToFriendlyDateString(this DateTime date)
        {
            string formattedDate;
            if (date.Date == DateTime.Today)
                formattedDate = "Today";

            else if (date.Date == DateTime.Today.AddDays(-1))
                formattedDate = "Yesterday";

            else if (date.Date > DateTime.Today.AddDays(MINUS_SIX))
                // *** Show the Day of the week
                formattedDate = date.ToString("dddd");

            else
                formattedDate = date.ToString("MMMM dd, yyyy");

            // append the time portion to the output
            formattedDate += " @ " + date.ToString("t").ToLower();
            return formattedDate;
        }

        /// <summary>
        /// Determines whether this date is between two sepcified dates.
        /// </summary>
        public static bool IsBetween(this DateTime date, DateTime from, DateTime to, bool includingEdges = true)
        {
            if (from > to)
                throw new ArgumentException("\"From\" date should be smaller than or equal to \"To\" date.");

            if (date < from || date > to) return false;

            if (!includingEdges)
                if (date == from || date == to) return false;

            return true;
        }

        /// <summary>
        /// Calculates the total working times in the specified duration which are between the two specified day-hours.
        /// This can be used to calculate working hours in a particular duration.
        /// </summary>
        public static TimeSpan CalculateTotalWorkingHours(this DateTime date, TimeSpan period, TimeSpan workingStartTime, TimeSpan workingEndTime, bool considerEnglishBankHolidays = true)
        {
            if (period < TimeSpan.Zero)
                throw new ArgumentException("duration should be a positive time span.");

            if (workingStartTime < TimeSpan.Zero || workingStartTime >= TimeSpan.FromHours(HOURS_IN_A_DAY))
                throw new ArgumentException("fromTime should be greater than or equal to 0, and smaller than 24.");

            if (workingEndTime <= TimeSpan.Zero || workingEndTime > TimeSpan.FromHours(HOURS_IN_A_DAY))
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
                workingTimesInday.Add(new KeyValuePair<TimeSpan, TimeSpan>(workingEndTime, TimeSpan.FromDays(1)));
                workingTimesInday.Add(new KeyValuePair<TimeSpan, TimeSpan>(TimeSpan.Zero, workingStartTime));
            }

            // For each working day in the range, calculate relevant times
            for (var day = date.Date; day < date.Add(period); day = day.AddWorkingDays(1, considerEnglishBankHolidays))
            {
                foreach (var range in workingTimesInday)
                {
                    var from = day.Add(range.Key);

                    if (from < date) from = date;

                    var to = day.Add(range.Value);
                    if (to < date) continue;

                    if (to > date.Add(period)) to = date.Add(period);

                    var amount = to - from;

                    if (amount < TimeSpan.Zero) continue;

                    result += amount;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the Date of the beginning of Quarter for this DateTime value (time will be 00:00:00).
        /// </summary>
        public static DateTime GetBeginningOfQuarter(this DateTime date)
        {
            var startMonths = new[] { 1, 4, 7, 10 };

            for (int i = startMonths.Length - 1; i >= 0; i--)
            {
                var beginningOfQuarter = new DateTime(date.Year, startMonths[i], 1);
                if (date >= beginningOfQuarter)
                    return beginningOfQuarter;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Returns the Date of the end of Quarter for this DateTime value (time will be 11:59:59).
        /// </summary>
        public static DateTime GetEndOfQuarter(this DateTime date) =>
            date.GetBeginningOfQuarter().AddMonths(4).GetBeginningOfQuarter().AddTicks(-1);

        /// <summary>
        /// Returns the Date of the end of Quarter for this DateTime value (time will be 11:59:59).
        /// </summary>
        public static DateTime GetEndOfMonth(this DateTime date) => date.GetBeginningOfMonth().AddMonths(1).AddTicks(-1);

        public static bool IsLastDayOfMonth(this DateTime date) => date.Date == date.GetEndOfMonth().Date;

        /// <summary>
        /// Gets the last date with the specified month and day.
        /// </summary>
        public static DateTime GetLast(this DateTime date, CalendarMonth month, int day)
        {
            date = date.Date; // cut time

            var thisYear = new DateTime(date.Year, (int)month, day);

            if (date >= thisYear) return thisYear;
            else return new DateTime(date.Year - 1, (int)month, day);
        }

        /// <summary>
        /// Gets the last date with the specified month and day.
        /// </summary>
        public static DateTime GetNext(this DateTime date, CalendarMonth month, int day)
        {
            date = date.Date; // cut time

            var thisYear = new DateTime(date.Year, (int)month, day);

            if (date >= thisYear) return new DateTime(date.Year + 1, (int)month, day);
            else return thisYear;
        }

        /// <summary>
        /// Returns the Date of the end of Quarter for this DateTime value (time will be 11:59:59).
        /// </summary>
        public static DateTime GetEndOfYear(this DateTime date) => new DateTime(date.Year + 1, 1, 1).AddTicks(-1);

        /// <summary>
        /// Gets the minimum value between this date and a specified other date.
        /// </summary>
        public static DateTime Min(this DateTime date, DateTime other)
        {
            if (other < date) return other;
            else return date;
        }

        /// <summary>
        /// Gets the maximum value between this date and a specified other date.
        /// </summary>
        public static DateTime Max(this DateTime date, DateTime other)
        {
            if (other > date) return other;
            else return date;
        }

        /// <summary>
        /// Adds the specified number of weeks and returns the result.
        /// </summary>
        public static DateTime AddWeeks(this DateTime date, int numberofWeeks) => date.AddDays(WEEK_DAYS_COUNT * numberofWeeks);

        /// <summary>
        /// Gets the latest date with the specified day of week and time that is before (or same as) this date.
        /// </summary>
        public static DateTime GetLast(this DateTime date, DayOfWeek day, TimeSpan timeOfDay)
        {
            var result = date.GetLast(day).Add(timeOfDay);

            if (result > date) return result.AddWeeks(-1);
            else return result;
        }

        /// <summary>
        /// Returns the local time equivalent of this UTC date value based on the TimeZone specified in Localtime.TimeZoneProvider.
        /// Use this instead of ToLocalTime() so you get control over the TimeZone.
        /// </summary>
        public static DateTime? ToLocal(this DateTime? utcValue) => utcValue?.ToLocal();

        /// <summary>
        /// Returns the local time equivalent of this UTC date value based on the TimeZone specified in Localtime.CurrentTimeZone().
        /// Use this instead of ToLocalTime() so you get control over the TimeZone.
        /// </summary>
        public static DateTime ToLocal(this DateTime utcValue) => utcValue.ToLocal(LocalTime.CurrentTimeZone());

        public static DateTime ToLocal(this DateTime utcValue, TimeZoneInfo timeZone) =>
            new DateTime(utcValue.Ticks, DateTimeKind.Local).Add(timeZone.GetUtcOffset(utcValue));

        /// <summary>
        /// Returns the equivalent Universal Time (UTC) of this local date value.
        /// </summary>
        public static DateTime? ToUniversal(this DateTime? localValue) => localValue?.ToUniversal();

        /// <summary>
        /// Returns the equivalent Universal Time (UTC) of this local date value.
        /// </summary>
        public static DateTime ToUniversal(this DateTime localValue) =>
            localValue.ToUniversal(sourceTimezone: LocalTime.CurrentTimeZone());

        /// <summary>
        /// Returns the equivalent Universal Time (UTC) of this local date value.
        /// </summary>
        public static DateTime ToUniversal(this DateTime localValue, TimeZoneInfo sourceTimezone) =>
            new DateTime(localValue.Ticks, DateTimeKind.Utc).Subtract(sourceTimezone.BaseUtcOffset);

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
        public static DateTime Round(this DateTime dateTime, TimeSpan nearest)
        {
            if (nearest == TimeSpan.Zero) return dateTime;

            var remainder = dateTime.Ticks % nearest.Ticks;

            var result = dateTime.AddTicks(-remainder);

            if (remainder >= nearest.Ticks / 2) result = result.Add(nearest);

            return result;
        }

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
        public static long ToUnixTime(this DateTime dateTime)
            => (int)(dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
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