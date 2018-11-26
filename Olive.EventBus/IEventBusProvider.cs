using System;
using System.Threading.Tasks;

namespace Olive
{
    public interface IEventBusProvider
    {
        Task<string> Publish(string queueKey, IEventBusMessage message);

        void Subscribe<TMessage>(string queueName, Func<TMessage, Task> @handler) where TMessage : IEventBusMessage;

        Task Purge(string queueKey);
    }
}
