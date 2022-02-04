using Microsoft.Extensions.DependencyInjection;

namespace Olive
{
    public static class RabbitMQEventBusExtensions
    {
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection @this)
        {
            return @this.AddTransient<IEventBusQueueProvider, RabbitMQ.EventBusProvider>();
        }
    }
}