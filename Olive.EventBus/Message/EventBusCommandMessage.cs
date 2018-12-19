using System;
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
    }
}
