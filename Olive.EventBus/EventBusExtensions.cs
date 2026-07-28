using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            return queue.Publish(Serialize(message));
        }

        /// <summary>
        /// Publishes the specified events to the current event bus provider.
        /// </summary>
        /// <returns>The unique id of the queue item.</returns>
        public static Task<IEnumerable<string>> PublishBatch(this IEventBusQueue queue, IEnumerable<IEventBusMessage> messages)
        {
            var stringMessages = new List<string>();

            messages.Do(message => stringMessages.Add(Serialize(message)));

            return queue.PublishBatch(stringMessages);
        }

        /// <summary>
        /// Serializes the message, stamping it with the reference code of the work publishing it so
        /// that the handler can adopt it. A message that already carries a code keeps it.
        /// </summary>
        static string Serialize(IEventBusMessage message)
        {
            var code = Log.CurrentReference;

            if (!(message is EventBusMessage carrier) || carrier.ReferenceCode.HasValue() || code.IsEmpty())
                return JsonConvert.SerializeObject(message);

            // Onto the serialized form, never onto the caller's object: a message published from two
            // threads at once would otherwise be seen mid-stamp, filing one request's work under another.
            var json = JObject.FromObject(message);
            json[nameof(EventBusMessage.ReferenceCode)] = code;

            return json.ToString(Formatting.None); // JToken.ToString() would otherwise indent it.
        }

        /// <summary>
        /// Reads the reference code out of a raw message body, without deserializing it to its type.
        /// </summary>
        public static string ReadReferenceCode(string message)
        {
            if (message.IsEmpty()) return null;

            try
            {
                using (var reader = new JsonTextReader(new StringReader(message)))
                    while (reader.Read())
                    {
                        // Depth 1 only: a nested object in the payload can have a field of this name.
                        if (reader.TokenType != JsonToken.PropertyName || reader.Depth != 1) continue;

                        if ((string)reader.Value != nameof(EventBusMessage.ReferenceCode))
                        {
                            reader.Skip();
                            continue;
                        }

                        return reader.Read() ? reader.Value?.ToString() : null;
                    }
            }
            catch (JsonException) { }   // Not JSON, or not ours. It gets a fresh code.

            return null;
        }

        /// <summary>
        /// How much of a message body may appear in the text of a failure. A whole one (up to 256KB)
        /// would push the log batch carrying it past what the event bus accepts.
        /// </summary>
        const int MaxLoggedBodyLength = 2000;

        /// <summary>A message body, cut down to what belongs in a log entry.</summary>
        public static string ForLog(string rawMessage)
        {
            // Summarize(enforceMaxLength) reads .Length off its own result, so it throws on null.
            if (rawMessage.IsEmpty()) return rawMessage;

            return rawMessage.Summarize(MaxLoggedBodyLength, enforceMaxLength: true);
        }

        /// <summary>
        /// Reports a queue handler failure under the current reference code, and returns it wrapped so
        /// the caller can act on it without reporting it a second time.
        /// </summary>
        public static LoggedException ReportFailure(this ILogger log, Exception error, string context, string rawMessage)
        {
            if (error is LoggedException already) return already;

            var failure = new Exception(context + " message: " + ForLog(rawMessage), error);

            log.Error(failure);

            return new LoggedException(failure);
        }

        public static LoggedException ReportFailure(this ILogger log, Exception error, MethodInfo handler, string rawMessage)
            => log.ReportFailure(error, "Failed to run queue event handler " +
                handler?.DeclaringType?.GetProgrammingName() + "." + handler?.Name, rawMessage);

        /// <summary>
        /// Runs a raw-message handler under the code the message was published with, reporting any
        /// failure inside that scope. For callers holding a message as a string rather than its type.
        /// </summary>
        public static async Task RunUnderReference(string rawMessage, Func<string, Task> handler)
        {
            // The scope covers the reporting, not just the handler: by the time the exception reaches
            // a caller the handler's own scope has unwound, and for a handler that simply throws this
            // is the only record of the failure there will be.
            using (Log.UseReference(ReadReferenceCode(rawMessage)))
            {
                try { await handler(rawMessage); }
                catch (Exception ex)
                {
                    throw Log.For(typeof(EventBusExtensions)).ReportFailure(ex, handler.Method, rawMessage);
                }
            }
        }

        /// <summary>
        /// Attaches an event handler to the specified queue message type.
        /// </summary>
        public static void Subscribe<TMessage>(this IEventBusQueue queue, Func<TMessage, Task> @handler)
            where TMessage : IEventBusMessage
        {
            queue.Subscribe(message => Handle(message, handler));
        }

        public static Task PullAll<TMessage>(this IEventBusQueue queue, Func<TMessage, Task> @handler)
        where TMessage : IEventBusMessage
        {
            return queue.PullAll(message => Handle(message, handler));
        }

        /// <summary>
        /// Runs the handler under the code the message was published with. A message carrying no code
        /// gets a fresh one, so that at least the handler's own logs are grouped together.
        /// </summary>
        static async Task Handle<TMessage>(string message, Func<TMessage, Task> handler)
            where TMessage : IEventBusMessage
        {
            if (message.IsEmpty()) return;

            TMessage @event;

            try { @event = JsonConvert.DeserializeObject<TMessage>(message); }
            catch (Exception ex)
            {
                throw new Exception("Failed to deserialize event message to " + typeof(TMessage).FullName + ":\r\n" + message, ex);
            }

            // Awaited, not returned: the scope must outlive the handler.
            using (Log.UseReference((@event as EventBusMessage)?.ReferenceCode))
                await handler(@event);
        }

        /// <summary>
        /// Pulls a single item from the specified queue, or null if nothing was available.
        /// After completing the message processing, you must call Complete().
        /// Unlike Subscribe and PullAll this opens no reference code scope, because the work happens in
        /// the caller; wrap that work in <see cref="QueueMessageHandle.UseReference"/>.
        /// </summary>
        public static async Task<QueueMessageHandle<TMessage>> Pull<TMessage>(this IEventBusQueue queue, int timeoutSeconds = 10)
           where TMessage : IEventBusMessage
        {
            var item = await queue.Pull(timeoutSeconds);
            if (item == null) return null;

            return item.As<TMessage>();
        }
    }
}