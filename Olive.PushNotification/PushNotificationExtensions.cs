using Microsoft.Extensions.DependencyInjection;

namespace Olive.PushNotification
{
    public static class PushNotificationExtensions
    {
        public static IServiceCollection AddPushNotification(this IServiceCollection @this)
        {
            return @this.AddSingleton<IPushNotificationService, PushNotificationService>();
        }
    }
}
