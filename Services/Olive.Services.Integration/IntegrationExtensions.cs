namespace Olive
{
    using Services.Integration;

    public static class IntegrationExtensions
    {
        public static bool IsInProcess(this IIntegrationQueueItem item) => item.DatePicked != null;

        public static bool IsProcessed(this IIntegrationQueueItem item) => item.ResponseDate != null;
    }
}
