using System;
using System.Threading.Tasks;

namespace Olive
{
    public class QueueMessageHandle
    {
        Func<Task> Completion;
        public QueueMessageHandle(string rawMessage, Func<Task> completion)
        {
            Completion = completion;
            RawMessage = rawMessage;
        }

        public Task Complete() => Completion();

        public string RawMessage { get; private set; }
    }

    public class QueueMessageHandle<TMessage> : QueueMessageHandle
    {
        internal QueueMessageHandle(string rawMessage, TMessage message, Func<Task> completion) : base(rawMessage, completion)
        {
            Message = message;
        }

        public TMessage Message { get; private set; }
    }
}
