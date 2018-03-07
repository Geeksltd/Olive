using Olive.Mvc.Testing;

namespace Olive.Email
{
    partial class EmailExtensions
    {
        public static IWebTestConfig AddEmail(this IWebTestConfig config)
        {
            config.Add("testEmail", async () =>
            {
                var service = new EmailTestService();
                await service.Initialize();
                await service.Process();
                return true;
            }, "Outbox...");

            return config;
        }
    }
}
