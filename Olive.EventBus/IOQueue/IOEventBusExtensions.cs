using Microsoft.Extensions.DependencyInjection;

namespace Olive
{
    public static class IOEventBusExtensions
    {
        public static IServiceCollection AddIOEventBus(this IServiceCollection @this)
        {
            return @this.AddTransient<IEventBusQueueProvider, IOEventBusProvider>();
        }
    }
}