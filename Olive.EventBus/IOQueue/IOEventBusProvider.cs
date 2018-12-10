namespace Olive
{
    class IOEventBusProvider : IEventBusQueueProvider
    {
        public IEventBusQueue Provide(string queueUrl) => new IOEventBusQueue(queueUrl);
    }
}
