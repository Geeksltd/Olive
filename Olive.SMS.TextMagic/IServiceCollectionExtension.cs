using Microsoft.Extensions.DependencyInjection;
using Olive.SMS;

namespace Olive.Aws.Ses
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddAwsSesProvider(this IServiceCollection @this)
        {
            return @this.AddTransient<ISmsDispatcher, Olive.SMS.TextMagic.SmsDispatcher>();
        }
    }
}