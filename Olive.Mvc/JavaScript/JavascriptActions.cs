using System.Collections.Generic;
using System.Linq;

namespace Olive.Mvc
{
    class JavascriptActions : List<object>
    {
        public new void Add(object action)
        {
            if (action != null) base.Add(action);
        }

        public void Redirect(string url, bool withAjax = false, string target = null)
        {
            url = url.Or("#");
            if (!url.OrEmpty().ToLower().StartsWithAny("/", "http:", "https:")) url = "/" + url;

            NotificationAction.ScheduleForNextRequest();

            Add(new { Redirect = url, WithAjax = withAjax, Target = target });
        }

        public void Do(WindowAction action)
        {
            if (this.OfType<NotificationAction>().Any())
                if (new[] { WindowAction.Refresh, WindowAction.CloseModalRefreshParent }.Contains(action))
                {
                    NotificationAction.ScheduleForNextRequest();
                }

            Add(new { BrowserAction = action.ToString() });
        }

        internal void ScheduleNotifications() => Add(NotificationAction.GetScheduledNotification());

        public void JavaScript(string script, PageLifecycleStage stage = PageLifecycleStage.Init)
            => Add(new { Script = script, Stage = stage.ToString() });
    }
}