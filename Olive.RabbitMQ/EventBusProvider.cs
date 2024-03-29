﻿using System.Collections.Concurrent;

namespace Olive.RabbitMQ
{
    public class EventBusProvider : IEventBusQueueProvider
    {
        static ConcurrentDictionary<string, IEventBusQueue> Cache = new ConcurrentDictionary<string, IEventBusQueue>();

        public IEventBusQueue Provide(string queueUrl) => Cache.GetOrAdd(queueUrl, u => new EventBusQueue(u));
    }
}
