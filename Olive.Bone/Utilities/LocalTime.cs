using System;

namespace Olive
{
    public class LocalTime
    {
        /// <summary>
        /// By default provides the current server's timezone.
        /// You can override this to provide user-specific time-zones or based on any other system setting.
        /// </summary>
        public static Func<TimeZoneInfo> CurrentTimeZone = () => TimeZoneInfo.Local;

        /// <summary> 
        /// If set, it will provide the "Now" value.
        /// Note: This has lower priority than thread-level overrides. 
        /// </summary>
        static Func<DateTime> GlobalNowGetter;

        /// <summary>
        /// Gets the local current date/time of the application.
        /// <para>By default it equals to System.DateTime.Now.</para>
        /// <para>To override its value, you should wrap the calling code inside "using (LocalTime.SetNow(some date)) { ... }"</para>
        /// <para>&#160;</para>
        /// <para> Examples:</para>
        /// <para>—————————————————————————————————</para>
        /// <para>var now = LocalTime.Now // which is identical to DateTime.Now</para>
        /// <para>—————————————————————————————————</para>
        /// <para>using (LocalTime.Set(DateTime.Parse("15/01/2000 06:13")))</para>
        /// <para>{</para>
        /// <para> var date = LocalTime.Now; // that sets date to 15th Jan 200 at 6:13.</para>
        /// <para>}</para>
        /// </summary>
        public static DateTime Now
        {
            get
            {
                var setting = ProcessContext<OverriddenApplicationDate>.Current;

                var nowGetter = setting?.NowGetter ?? GlobalNowGetter;

                if (nowGetter != null) return nowGetter();

                return DateTime.UtcNow.ToLocal();
            }
        }

        /// <summary>
        /// Gets the current Universal Time.
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                var setting = ProcessContext<OverriddenApplicationDate>.Current;

                var nowGetter = setting?.NowGetter ?? GlobalNowGetter;

                if (nowGetter != null) return nowGetter().ToUniversal();

                return DateTime.UtcNow;
            }
        }

        public static bool IsRedefined => (ProcessContext<OverriddenApplicationDate>.Current?.NowGetter ?? GlobalNowGetter) != null;

        /// <summary>
		/// Gets the local current date of the application (no time).
        /// <para>By default it equals to System.DateTime.Today.</para>
        /// <para>To override its value, you should wrap the calling code inside "using (LocalTime.SetNow(some date)) { ... }"</para>
        /// <para>&#160;</para>
        /// <para> Examples:</para>
        /// <para>—————————————————————————————————</para>
        /// <para>var now = LocalTime.Today // which is identical to DateTime.Today</para>
        /// <para>—————————————————————————————————</para>
        /// <para>using (LocalTime.Set(DateTime.Parse("15/01/2000 06:13")))</para>
        /// <para>{</para>
        /// <para> var date = LocalTime.Today; // that sets date to 15th Jan 200.</para>
        /// <para>}</para>
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// Gets the current Universal Time's date part (without time).
        /// </summary>
        public static DateTime UtcToday => UtcNow.Date;

        /// <summary>
		/// Sets the current time of the application.
        /// <para>&#160;</para>
        /// <para> Examples:</para>
        /// <para>—————————————————————————————————</para>
        /// <para>using (LocalTime.Set(DateTime.Parse("15/01/2000 06:13")))</para>
        /// <para>{</para>
        /// <para><tab> </tab>//Here any call for LocalTime.Now/Today will return 15th of Jan 2000 (at 6:30).</para>
        /// <para>}</para>
        /// </summary>
        public static IDisposable Set(DateTime overriddenNow) => Set(() => overriddenNow);

        /// <summary>
        /// Sets the current time function of the application.
        /// </summary>
        public static IDisposable Set(Func<DateTime> overriddenNow) =>
            new ProcessContext<OverriddenApplicationDate>(new OverriddenApplicationDate(overriddenNow));

        /// <summary>
        /// Sets the current time function of the application.
        /// Note: This has lower priority than thread-level time setting.
        /// </summary>
        public static void RedefineNow(Func<DateTime> overriddenNow) => GlobalNowGetter = overriddenNow;

        /// <summary>
        /// Freezes the time to the current system time.
        /// <para>&#160;</para>
        /// <para> Examples:</para>
        /// <para>—————————————————————————————————</para>
        /// <para>using (LocalTime.Stop())</para>
        /// <para>{</para>
        /// <para> // Freezes the time to Datetime.Now.</para>
        /// <para>}</para>
        /// </summary>
        [EscapeGCop("I AM the solution myself!")]
        public static IDisposable Stop() => Set(DateTime.Now);

        /// <summary>
        /// Adds the specified time to the current LocalTime.
        /// </summary>
        public static void Add(TimeSpan time)
        {
            var setting = ProcessContext<OverriddenApplicationDate>.Current;

            if (setting == null)
                throw new InvalidOperationException("The current thread is not running inside a LocalTime.");

            var currentGetter = setting.NowGetter;
            setting.NowGetter = () => currentGetter.Invoke().Add(time);
        }

        public static void AddSeconds(double seconds) => Add(TimeSpan.FromSeconds(seconds));

        public static void AddMinutes(double minutes) => Add(TimeSpan.FromMinutes(minutes));

        public static void AddHours(double hours) => Add(TimeSpan.FromHours(hours));

        public static void AddDays(double days) => Add(TimeSpan.FromDays(days));
    }

    class OverriddenApplicationDate
    {
        public Func<DateTime> NowGetter { get; internal set; }

        /// <summary>
        /// Creates a new OverriddenApplicationDate instance.
        /// </summary>
        public OverriddenApplicationDate(Func<DateTime> time) => NowGetter = time;
    }
}