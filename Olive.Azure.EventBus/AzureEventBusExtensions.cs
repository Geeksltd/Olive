using Microsoft.Extensions.DependencyInjection;

namespace Olive
{
    public static class AzureEventBusExtensions
    {
        public static IServiceCollection AddAwsEventBus(this IServiceCollection @this)
        {
            return @this.AddTransient<IEventBusQueueProvider, Azure.EventBusProvider>();
        }
    }
}