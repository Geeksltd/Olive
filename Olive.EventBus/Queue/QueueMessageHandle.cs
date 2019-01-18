using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    public class QueueMessageHandle<TMessage>
    {
        Func<Task> Completion;
        public QueueMessageHandle(TMessage message, Func<Task> completion)
        {
            Message = message;
            Completion = completion;
        }

        public TMessage Message { get; private set; }

        public Task Complete() => Completion();
    }
}
