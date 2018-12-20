using System;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// Represents a message sent or received using a queue-based communication.
    /// </summary>
    public interface IEventBusMessage
    {
        /// <summary>
        /// The ID used by queue systems to deduplicate a message in a FIFO (first-in-first-out) queue. (in cases of double delivery).
        /// </summary>
        string DeduplicationId { get; }
    }
}