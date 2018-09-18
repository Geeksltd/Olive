using Microsoft.Extensions.DependencyInjection;

namespace Olive.SMS
{
    public static class SmsExtensions
    {
        public static IServiceCollection AddSms(this IServiceCollection @this)
        {
            return @this
                 .AddSingleton<ISmsService, SmsService>();
        }
    }
}