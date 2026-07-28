using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        /// <summary>The reference code this message was published under, or null if it carries none.</summary>
        public string ReferenceCode => EventBusExtensions.ReadReferenceCode(RawMessage);

        /// <summary>
        /// Runs the handling of this message under the reference code it was published with. Subscribe
        /// and PullAll do this for you; a message obtained from Pull is handled by the caller, so the
        /// caller must open the scope.
        /// </summary>
        public IDisposable UseReference() => Log.UseReference(ReferenceCode);

        public QueueMessageHandle<TMessage> As<TMessage>() where TMessage : IEventBusMessage
        {
            if (RawMessage.IsEmpty()) return null;

            try
            {
                var message = JsonConvert.DeserializeObject<TMessage>(RawMessage);
                return new QueueMessageHandle<TMessage>(RawMessage, MessageId, message, Complete);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize event message to " + typeof(TMessage).FullName + ":\r\n" + RawMessage, ex);
            }
        }
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
