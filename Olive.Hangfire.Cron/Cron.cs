using System;

namespace Hangfire
{
    //
    // Summary:
    //     Helper class that provides common values for the cron expressions.
    public static class Cron
    {
        //
        // Summary:
        //     Returns cron expression that fires every minute.
        public static string Minutely()
        {
            return "* * * * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every hour at the first minute.
        public static string Hourly()
        {
            return Hourly(0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every hour at the specified minute.
        //
        // Parameters:
        //   minute:
        //     The minute in which the schedule will be activated (0-59).
        public static string Hourly(int minute)
        {
            return $"{minute} * * * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every day at 00:00 UTC.
        public static string Daily()
        {
            return Daily(0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every day at the first minute of the specified
        //     hour in UTC.
        //
        // Parameters:
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        public static string Daily(int hour)
        {
            return Daily(hour, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every day at the specified hour and minute
        //     in UTC.
        //
        // Parameters:
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        //
        //   minute:
        //     The minute in which the schedule will be activated (0-59).
        public static string Daily(int hour, int minute)
        {
            return $"{minute} {hour} * * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every week at Monday, 00:00 UTC.
        public static string Weekly()
        {
            return Weekly(DayOfWeek.Monday);
        }

        //
        // Summary:
        //     Returns cron expression that fires every week at 00:00 UTC of the specified day
        //     of the week.
        //
        // Parameters:
        //   dayOfWeek:
        //     The day of week in which the schedule will be activated.
        public static string Weekly(DayOfWeek dayOfWeek)
        {
            return Weekly(dayOfWeek, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every week at the first minute of the specified
        //     day of week and hour in UTC.
        //
        // Parameters:
        //   dayOfWeek:
        //     The day of week in which the schedule will be activated.
        //
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        public static string Weekly(DayOfWeek dayOfWeek, int hour)
        {
            return Weekly(dayOfWeek, hour, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every week at the specified day of week, hour
        //     and minute in UTC.
        //
        // Parameters:
        //   dayOfWeek:
        //     The day of week in which the schedule will be activated.
        //
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        //
        //   minute:
        //     The minute in which the schedule will be activated (0-59).
        public static string Weekly(DayOfWeek dayOfWeek, int hour, int minute)
        {
            return $"{minute} {hour} * * {(int)dayOfWeek}";
        }

        //
        // Summary:
        //     Returns cron expression that fires every month at 00:00 UTC of the first day
        //     of month.
        public static string Monthly()
        {
            return Monthly(1);
        }

        //
        // Summary:
        //     Returns cron expression that fires every month at 00:00 UTC of the specified
        //     day of month.
        //
        // Parameters:
        //   day:
        //     The day of month in which the schedule will be activated (1-31).
        public static string Monthly(int day)
        {
            return Monthly(day, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every month at the first minute of the specified
        //     day of month and hour in UTC.
        //
        // Parameters:
        //   day:
        //     The day of month in which the schedule will be activated (1-31).
        //
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        public static string Monthly(int day, int hour)
        {
            return Monthly(day, hour, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every month at the specified day of month,
        //     hour and minute in UTC.
        //
        // Parameters:
        //   day:
        //     The day of month in which the schedule will be activated (1-31).
        //
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        //
        //   minute:
        //     The minute in which the schedule will be activated (0-59).
        public static string Monthly(int day, int hour, int minute)
        {
            return $"{minute} {hour} {day} * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every year on Jan, 1st at 00:00 UTC.
        public static string Yearly()
        {
            return Yearly(1);
        }

        //
        // Summary:
        //     Returns cron expression that fires every year in the first day at 00:00 UTC of
        //     the specified month.
        //
        // Parameters:
        //   month:
        //     The month in which the schedule will be activated (1-12).
        public static string Yearly(int month)
        {
            return Yearly(month, 1);
        }

        //
        // Summary:
        //     Returns cron expression that fires every year at 00:00 UTC of the specified month
        //     and day of month.
        //
        // Parameters:
        //   month:
        //     The month in which the schedule will be activated (1-12).
        //
        //   day:
        //     The day of month in which the schedule will be activated (1-31).
        public static string Yearly(int month, int day)
        {
            return Yearly(month, day, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every year at the first minute of the specified
        //     month, day and hour in UTC.
        //
        // Parameters:
        //   month:
        //     The month in which the schedule will be activated (1-12).
        //
        //   day:
        //     The day of month in which the schedule will be activated (1-31).
        //
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        public static string Yearly(int month, int day, int hour)
        {
            return Yearly(month, day, hour, 0);
        }

        //
        // Summary:
        //     Returns cron expression that fires every year at the specified month, day, hour
        //     and minute in UTC.
        //
        // Parameters:
        //   month:
        //     The month in which the schedule will be activated (1-12).
        //
        //   day:
        //     The day of month in which the schedule will be activated (1-31).
        //
        //   hour:
        //     The hour in which the schedule will be activated (0-23).
        //
        //   minute:
        //     The minute in which the schedule will be activated (0-59).
        public static string Yearly(int month, int day, int hour, int minute)
        {
            return $"{minute} {hour} {day} {month} *";
        }

        //
        // Summary:
        //     Returns cron expression that never fires. Specifically 31st of February
        public static string Never()
        {
            return Yearly(2, 31);
        }

        //
        // Summary:
        //     Returns cron expression that fires every <interval> minutes.
        //
        // Parameters:
        //   interval:
        //     The number of minutes to wait between every activation.
        public static string MinuteInterval(int interval)
        {
            return $"*/{interval} * * * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every <interval> hours.
        //
        // Parameters:
        //   interval:
        //     The number of hours to wait between every activation.
        public static string HourInterval(int interval)
        {
            return $"0 */{interval} * * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every <interval> days.
        //
        // Parameters:
        //   interval:
        //     The number of days to wait between every activation.
        public static string DayInterval(int interval)
        {
            return $"0 0 */{interval} * *";
        }

        //
        // Summary:
        //     Returns cron expression that fires every <interval> months.
        //
        // Parameters:
        //   interval:
        //     The number of months to wait between every activation.
        public static string MonthInterval(int interval)
        {
            return $"0 0 1 */{interval} *";
        }
    }
}