using System;

namespace Olive.Entities.Replication
{
    /// <summary>
    /// When applied to a DataEndPoint sub-class, it specifies the queue url configurtion for it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventBusAttribute : Attribute
    {
        public string Environment { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// Specifies the Queue url for the messages to send from this data end point.
        /// </summary>
        /// <param name="environment">One of the values from Microsoft.Extensions.Hosting.Environments.</param>
        /// <param name="url">The url of the queue where the messages will be sent.
        /// The approprite EventBus implementation will be automatically picked based on the URL pattern (e.g. AWS SQS or Microsoft Azure).</param>
        public EventBusAttribute(string environment, string url)
        {
            Environment = environment;
            Url = url;
        }
    }
}