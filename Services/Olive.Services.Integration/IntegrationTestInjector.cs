using System;
using System.Threading;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Services.Integration
{
    public class IntegrationTestInjector
    {
        const int INJECTOR_AGENT_WAIT_INTERVALS = 10;// ms

        public static async Task Inject(Type serviceType, string request, string response)
        {
            var serviceKey = IntegrationManager.GetServiceKey(serviceType);

            while (true)
            {
                var queueItem = await Entity.Database.FirstOrDefault<IIntegrationQueueItem>(i =>
                    i.IntegrationService == serviceKey &&
                    i.ResponseDate == null &&
                    (request.IsEmpty() || i.Request == request));

                if (queueItem == null)
                {
                    Thread.Sleep(INJECTOR_AGENT_WAIT_INTERVALS);
                    continue;
                }

                await Entity.Database.Update(queueItem, i =>
                {
                    i.Response = response;
                    i.ResponseDate = LocalTime.Now;
                });

                break;
            }
        }
    }
}
