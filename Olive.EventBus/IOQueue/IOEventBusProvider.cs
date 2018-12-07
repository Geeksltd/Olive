using System;
using System.Collections.Generic;
using System.Text;

namespace Olive
{
    class IOEventBusProvider : IEventBusQueueProvider
    {
        public IEventBusQueue Provide(string queueUrl)
        {
            throw new NotImplementedException();
        }
    }
}
