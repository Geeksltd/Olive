using System;

namespace Olive
{
    public class ApiClientEvent
    {
        /// <summary>
        /// End-user will see this message, you can change this message.
        /// </summary>
        public string FriendlyMessage { get; set; }

        public string ExceptionMessage { get; set; }

        public TimeSpan? CacheAge { get; set; }

        public string Url { get; set; }
    }

    public class UsingCacheInsteadOfFreshEvent : ApiClientEvent
    {

    }
}