using System;
using System.IO;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// A self-fulfilling event bus message.
    /// </summary>
    public abstract class EventBusCommandMessage : EventBusMessage
    {
        /// <summary>
        /// Will handle the received message. It should not hide exceptions.
        /// </summary>
        public abstract Task Process();

        /// <summary>
        /// Deserializes a command off the wire and runs it under the reference code it was published
        /// with. This is the path a command actually arrives on — an SQS event dispatches straight to
        /// here, bypassing the typed Subscribe — so the scope and the reporting both belong here.
        /// </summary>
        public static async Task Process(string message, Type eventBusCommandMessageType)
        {
            var command = (EventBusCommandMessage)Newtonsoft.Json.JsonConvert
                .DeserializeObject(message, eventBusCommandMessageType);

            if (command == null) return;

            using (Log.UseReference(command.ReferenceCode))
            {
                try { await command.Process(); }
                catch (Exception ex)
                {
                    // Rethrown so the message is not acked: SQS redelivers, and eventually dead-letters.
                    throw Log.For<EventBusCommandMessage>().ReportFailure(ex,
                        "Failed to process command " + eventBusCommandMessageType.FullName, message);
                }
            }
        }
    }
}
