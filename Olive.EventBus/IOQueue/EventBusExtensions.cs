using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive
{
    public static class EventBusExtensions
    {
        /// <summary>
        /// Publishes the specified event to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public static Task<string> Publish(this IEventBusQueue queue, IEventBusMessage message)
        {
            var textMessage = JsonConvert.SerializeObject(message);
            return queue.Publish(textMessage);
        }

        /// <summary>
        /// Publishes the specified events to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public static Task<IEnumerable<string>> PublishBatch(this IEventBusQueue queue, IEnumerable<IEventBusMessage> messages)
        {
            var stringMessegas = new List<string>();

            messages.Do(message => stringMessegas.Add(JsonConvert.SerializeObject(message)));

            return queue.PublishBatch(stringMessegas);
        }

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        public static void Subscribe<TMessage>(this IEventBusQueue queue, Func<TMessage, Task> @handler)
            where TMessage : IEventBusMessage
        {
            queue.Subscribe(message =>
            {
                if (message.IsEmpty()) return Task.CompletedTask;
                try
                {
                    var @event = JsonConvert.DeserializeObject<TMessage>(message);
                    return handler(@event);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to deserialize event message to " + typeof(TMessage).FullName + ":\r\n" + message, ex);
                }
            });
        }


        public static Task PullAll<TMessage>(this IEventBusQueue queue, Func<TMessage, Task> @handler)
        where TMessage : IEventBusMessage
        {
            return queue.PullAll(message =>
            {
                if (message.IsEmpty()) return Task.CompletedTask;
                try
                {
                    var @event = JsonConvert.DeserializeObject<TMessage>(message);
                    return handler(@event);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to deserialize event message to " + typeof(TMessage).FullName + ":\r\n" + message, ex);
                }
            });
        }

        /// <summary>
        /// Pulls a single item from the specified queue, or null if nothing was available.
        /// After completing the message processing, you must call Complete().
        /// </summary>
        public static async Task<QueueMessageHandle<TMessage>> Pull<TMessage>(this IEventBusQueue queue, int timeoutSeconds = 10)
           where TMessage : IEventBusMessage
        {
            var item = await queue.Pull(timeoutSeconds);
            if (item == null || item.RawMessage.IsEmpty()) return null;

            try
            {
                var message = JsonConvert.DeserializeObject<TMessage>(item.RawMessage);
                return new QueueMessageHandle<TMessage>(item.RawMessage, item.MessageId, message, () => item.Complete());
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize event message to " + typeof(TMessage).FullName + ":\r\n" + item.RawMessage, ex);
            }
        }
    }
}