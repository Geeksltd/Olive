using Microsoft.Extensions.DependencyInjection;
using Olive.Email;

namespace Olive.Email.Microsoft365
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddMicrosoft365(this IServiceCollection @this)
        {
            return @this.AddTransient<IEmailDispatcher, Microsoft365EmailDispatcher>();
        }
    }
}
