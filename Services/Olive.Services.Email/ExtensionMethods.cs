using System;
using System.Threading.Tasks;
using Olive.Entities;
using Olive.Web;

namespace Olive.Services.Email
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static Task<IEmailQueueItem> SendAsNotification(this Exception error) =>
            SendAsNotification(error, Config.Get("Error.Notification.Receiver"));

        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static async Task<IEmailQueueItem> SendAsNotification(this Exception error, string toNotify)
        {
            var context = Context.HttpContextAccessor.HttpContext;

            if (toNotify.IsEmpty())
                return null;
            var email = EmailService.EmailQueueItemFactory();
            email.To = toNotify;
            email.Subject = "Error In Application";
            email.Body = $"URL: {context?.Request?.ToRawUrl()}{Environment.NewLine}" +
                $"IP: {context?.Connection.RemoteIpAddress}{Environment.NewLine}" +
                $"User: {ApplicationEventManager.GetCurrentUserId(context?.User)}{Environment.NewLine}" +
                error.ToLogString(error.Message);
            await Entity.Database.Save(email);
            return email;
        }
    }
}
