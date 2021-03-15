using Microsoft.Extensions.DependencyInjection;

namespace Olive.PushNotification
{
    public static class PushNotificationExtensions
    {
        public static IServiceCollection AddPushNotification(this IServiceCollection @this)
        {
            return @this.AddSingleton<IPushNotificationService, PushNotificationService>()
                        .AddSingleton<ISubscriptionIdResolver, NullSubscriptionIdResolver>();
        }

        public static IServiceCollection AddPushNotification<TResolver>(this IServiceCollection @this) where TResolver : class, ISubscriptionIdResolver
        {
            return @this.AddSingleton<IPushNotificationService, PushNotificationService>()
                        .AddSingleton<ISubscriptionIdResolver, TResolver>();
        }
    }
}
