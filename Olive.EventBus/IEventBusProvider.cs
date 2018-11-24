using System;
using System.Threading.Tasks;

namespace Olive
{
    public interface IEventBusProvider
    {
        void Subscribe<TMessage>(Func<TMessage, Task> @handler) where TMessage : IEventBusMessage;

        Task<string> Publish<TMessage>(TMessage message) where TMessage : IEventBusMessage;
    }
}
