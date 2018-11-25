using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    public class EventBusProvider : IEventBusProvider
    {
        public Task<string> Publish(string queueKey, IEventBusMessage message)
            => new Publisher(queueKey).Publish(message);

        public void Subscribe<TMessage>(string queueKey, Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
        {
            new Subscriber<TMessage>(queueKey, handler).Start();
        }
    }
}
