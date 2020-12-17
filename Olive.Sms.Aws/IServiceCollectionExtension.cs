using Microsoft.Extensions.DependencyInjection;

namespace Olive.SMS
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddAwsSms(this IServiceCollection @this)
        {
            return @this.AddTransient<ISmsDispatcher, SmsDispatcher>();
        }
    }
}