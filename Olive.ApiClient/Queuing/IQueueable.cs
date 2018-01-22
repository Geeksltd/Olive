using System;

namespace Olive
{
    partial class ApiClient
    {
        /// <summary>
        /// Represents an Queueable Entity with unique identifier.
        /// </summary>
        public interface IQueueable<T>
        {
            /// <summary>The unique identifiere.</summary>
            T ID { get; set; }

            /// <summary>Gets the ID of this entity.</summary>     
            QueueStatus Status { get; set; }

            /// <summary>Time Item added to Queue</summary>  
            DateTime TimeAdded { get; set; }

            /// <summary>Latest update time of Item status in Queue</summary>  
            DateTime TimeUpdated { get; set; }

            /// <summary>
            /// Request info that should be sent when the app got connected to network
            /// </summary>  
            RequestInfo RequestInfo { get; set; }
        }
    }
}