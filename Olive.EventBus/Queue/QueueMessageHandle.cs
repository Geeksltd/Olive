using System;
using System.Threading.Tasks;

namespace Olive
{
    public class QueueMessageHandle
    {

        Func<Task> Completion;
        public QueueMessageHandle(string rawMessage, string messageId, Func<Task> completion)
        {
            Completion = completion;
            RawMessage = rawMessage;
            MessageId = messageId;
        }

        public Task Complete() => Completion();

        public string RawMessage { get; private set; }
        public string MessageId { get; private set; }
    }

    public class QueueMessageHandle<TMessage> : QueueMessageHandle
    {
        internal QueueMessageHandle(string rawMessage, string messageId, TMessage message, Func<Task> completion) : base(rawMessage, messageId, completion)
        {
            Message = message;
        }

        public TMessage Message { get; private set; }
    }
}
