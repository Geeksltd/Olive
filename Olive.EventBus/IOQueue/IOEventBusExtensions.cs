using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Olive
{
    public static class IOEventBusExtensions
    {
        public static IServiceCollection AddIOEventBus(this IServiceCollection @this)
        {
            return @this.AddTransient<IEventBusQueueProvider, IOEventBusProvider>();
        }

        /// <summary>
        /// Registers a command-based event bus message handler that handles itself.
        /// </summary>
        public static void Subscribe<TCommandMessage>(this EventBusQueue<TCommandMessage> @this)
            where TCommandMessage : EventBusCommandMessage, new()
        {
            @this.Subscribe(x => x.Process());
        }
        public static async Task PullAll<TCommandMessage>(this EventBusQueue<TCommandMessage> @this)
          where TCommandMessage : EventBusCommandMessage, new()
        {
            await @this.PullAll(x => x.Process());
        }
    }
}
