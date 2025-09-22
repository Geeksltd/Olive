using Newtonsoft.Json;
using Olive.Web;
using System.Linq;

namespace Olive.Mvc
{
    class NotificationAction
    {
        public string Notify { get; set; }
        public bool Obstruct { get; set; }
        public bool AsModal { get; set; }
        public string Style { get; set; }

        const string COOKIE_KEY = "M#.Scheduled.Notifications";

        /// <summary>
        /// Removes the notification actions from the current set of specified actions, and schedules them in the next request through the cookie.
        /// </summary>
        internal static void ScheduleForNextRequest()
        {
            var js = Context.Current.Http().JavascriptActions();
            var notificationActions = js.OfType<NotificationAction>().ToList();
            notificationActions.Do(a => js.Remove(a));

            if (notificationActions.None()) return;

            var json = JsonConvert.SerializeObject(new NotificationAction
            {
                Notify = notificationActions.Select(x => x.Notify).ToLinesString(),
                Obstruct = notificationActions.First().Obstruct,
                AsModal = notificationActions.Any(a => a.AsModal),
                Style = notificationActions.First().Style
            });

            CookieProperty.Set(COOKIE_KEY, json);
        }

        internal static NotificationAction GetScheduledNotification()
        {
            var value = CookieProperty.Get(COOKIE_KEY).GetAlreadyCompletedResult();

            if (value.IsEmpty()) return null;

            CookieProperty.Remove(COOKIE_KEY);

            return JsonConvert.DeserializeObject<NotificationAction>(value);
        }
    }
}
