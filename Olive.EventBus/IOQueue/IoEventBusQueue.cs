using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    class IOEventBusQueue : IEventBusQueue
    {
        public Task<string> Publish(IEventBusMessage message) => throw new NotImplementedException();
        public Task Purge() => throw new NotImplementedException();
        public void Subscribe<TMessage>(Func<TMessage, Task> handler) where TMessage : IEventBusMessage => throw new NotImplementedException();
    }
}
