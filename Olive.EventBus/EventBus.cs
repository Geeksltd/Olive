using System;

namespace Olive
{
    public static class EventBus
    {
        static IEventBusQueueProvider Provider => Context.Current.GetService<IEventBusQueueProvider>();

        /// <summary>
        /// Returns a queue for a specified queue url.
        /// </summary>
        public static IEventBusQueue Queue(string queueUrl) => Provider.Provide(queueUrl.Trim());
        public static string QueueUrl(Type messageType)
        {
            var url = Config.Get($"EventBus:Queues:{messageType.FullName.Replace(".", "_")}:Url");

            if (url.IsEmpty())
                url = Config.GetOrThrow($"EventBus:Queues:{messageType.FullName}:Url");

            return url;
        }

        /// <summary>
        /// Returns a queue for a specified message type. The Url of the queue is expected to be in the config file, under the setting of
        /// EventBus:Queues:{NameSpace}.{MessageClassName}:Url
        /// </summary>
        public static EventBusQueue<TMessage> Queue<TMessage>() where TMessage : IEventBusMessage
        {
            return new EventBusQueue<TMessage>(Queue(QueueUrl(typeof(TMessage))));
        }

        /// <summary>
        /// Returns a queue for a specified message type. The Url of the queue is expected to be in the config file, under the setting of
        /// EventBus:Queues:{NameSpace}.{MessageClassName}:Url
        /// </summary>
        public static IEventBusQueue Queue(Type messageType)
        {
            return Queue(QueueUrl(messageType));
        }

        /// <summary>
        /// Returns a URL that, when called, ensures all messages in the queue are pulled and processed.
        /// </summary>
        public static string ProcessCommandUrl<TMessage>() where TMessage : IEventBusMessage
        {
            return ProcessCommandUrl(typeof(TMessage));
        }

        /// <summary>
        /// Returns a URL that, when called, ensures all messages in the queue are pulled and processed.
        /// </summary>
        public static string ProcessCommandUrl(Type commandType)
        {
            var commandName = commandType.FullName;
            var consumer = Config.GetOrThrow($"EventBus:Queues:{commandName}:Consumer");
            var consumerUrl = Config.GetOrThrow($"Microservice:{consumer}:Url");
            return $"{consumerUrl.TrimEnd("/")}/olive/process-command/{commandName}";
        }

        /// <summary>
        /// Returns a queue for a specified queue url.
        /// </summary>
        public static EventBusQueue<TMessage> Queue<TMessage>(string url) where TMessage : IEventBusMessage
        {
            return new EventBusQueue<TMessage>(Queue(url));
        }
    }
}
