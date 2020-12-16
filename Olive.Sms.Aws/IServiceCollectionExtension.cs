using Microsoft.Extensions.DependencyInjection;
using Olive.SMS;

namespace Olive.SMS.TextMagic
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddAwsSms(this IServiceCollection @this)
        {
            return @this.AddTransient<ISmsDispatcher, SmsDispatcher>();
        }
    }
}