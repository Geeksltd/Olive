using Microsoft.Extensions.DependencyInjection;

namespace Olive.SMS.MessageBird
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddMessageBird(this IServiceCollection @this)
        {
            return @this.AddTransient<ISmsDispatcher, SmsDispatcher>();
        }
    }
}
