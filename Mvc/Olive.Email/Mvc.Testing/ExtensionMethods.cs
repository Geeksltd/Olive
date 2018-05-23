namespace Olive.Email
{
    partial class EmailExtensions
    {
        public static IDevCommandsConfig AddEmail(this IDevCommandsConfig config)
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
