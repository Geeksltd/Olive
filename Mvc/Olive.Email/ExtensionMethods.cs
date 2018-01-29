using System;
using System.Threading.Tasks;
using Olive.Entities;
using Olive.Mvc.Testing;
using Olive.Web;

namespace Olive.Email
{
    public static class ExtensionMethods
    {
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
