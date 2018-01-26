using Olive.Services.Testing;
using Olive.Web;
using System;

namespace Olive.Services.Email
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static void SendAsNotification(this Exception error) => Olive.Log.Error("Notify", error);

        public static IWebTestConfig AddEmail(this IWebTestConfig config)
        {
            config.Add("testEmail", async () =>
            {
                var service = new EmailTestService(Context.Request, Context.Response);
                await service.Initialize();
                await service.Process();
                return true;
            }, "Outbox...");

            return config;
        }
    }
}
