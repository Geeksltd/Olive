using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Web;

namespace Olive.Mvc
{
    class NotificationAction
    {
        public string Notify { get; set; }
        public bool Obstruct { get; set; }
        public string Style { get; set; }

        const string COOKIE_KEY = "M#.Scheduled.Notifications";

        /// <summary>
        /// Removes the notification actions from the current set of specified actions, and schedules them in the next request through the cookie.
        /// </summary>
        internal static void ScheduleForNextRequest(List<object> actions)
        {
            var notificationActions = actions.OfType<NotificationAction>().ToList();

            if (notificationActions.None()) return;

            notificationActions.Do(a => actions.Remove(a));

            var json = JsonConvert.SerializeObject(new NotificationAction
            {
                Notify = notificationActions.Select(x => x.Notify).ToLinesString(),
                Obstruct = notificationActions.First().Obstruct,
                Style = notificationActions.First().Style
            });

            CookieProperty.Set(COOKIE_KEY, json);
        }

        internal static async Task<NotificationAction> GetScheduledNotification()
        {
            var value = await CookieProperty.Get(COOKIE_KEY);

            if (value.IsEmpty()) return null;

            CookieProperty.Remove(COOKIE_KEY);

            return JsonConvert.DeserializeObject<NotificationAction>(value);
        }
    }
}
