using Microsoft.EntityFrameworkCore;

namespace Olive.Entities.EF.Replication;

public static class EfReplicationConsumerExtensions
{
    /// <summary>
    /// Starts SQS/IO polling for a <see cref="DestinationEndpoint"/> when its queue URL is configured.
    /// </summary>
    public static Task SubscribeIfConfigured(
        bool isQueueConfigured,
        Func<DestinationEndpoint> createEndpoint,
        bool isRefreshMessageRequired = false)
    {
        if (!isQueueConfigured)
            return Task.CompletedTask;

        return createEndpoint().Subscribe(isRefreshMessageRequired);
    }
}
