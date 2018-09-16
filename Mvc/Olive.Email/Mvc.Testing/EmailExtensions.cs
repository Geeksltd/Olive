namespace Olive.Email
{
    partial class EmailExtensions
    {
        public static IDevCommandsConfig AddEmail(this IDevCommandsConfig config)
        {
            var service = Context.Current.GetService<EmailTestService>();

            config.Add("testEmail", async () =>
            {
                await service.Initialize();
                await service.Process();
                return true;
            }, "Outbox...");

            return config;
        }
    }
}