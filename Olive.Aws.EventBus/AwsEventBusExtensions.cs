using Microsoft.Extensions.DependencyInjection;

namespace Olive
{
    public static class AwsEventBusExtensions
    {
        public static IServiceCollection AddAwsEventBus(this IServiceCollection @this)
        {
            return @this.AddTransient<IEventBusQueueProvider, Aws.EventBusProvider>();
        }
    }
}