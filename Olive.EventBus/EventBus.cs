using System;

namespace Olive
{
    public static class EventBus
    {
        static IEventBusQueueProvider Provider => Context.Current.GetService<IEventBusQueueProvider>();

        public static IEventBusQueue Queue(string queueUrl) => Provider.Provide(queueUrl);
    }
}
