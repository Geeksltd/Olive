using System;
using System.Threading.Tasks;

namespace Olive
{
    public interface IEventBusQueueProvider
    {
        IEventBusQueue Provide(string queueUrl);
    }
}
