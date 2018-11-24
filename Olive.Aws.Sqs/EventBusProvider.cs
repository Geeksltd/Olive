using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    public class EventBusProvider : IEventBusProvider
    {
        public Task<string> Publish<TMessage>(TMessage message)
            where TMessage : IEventBusMessage
            => new Publisher<TMessage>().Publish(message);

        public void Subscribe<TMessage>(Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
        {
            new Subscriber<TMessage>(handler).Start();
        }
    }
}
