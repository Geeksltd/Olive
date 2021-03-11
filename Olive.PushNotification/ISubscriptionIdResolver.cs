using System.Threading.Tasks;

namespace Olive.PushNotification
{
    /// <summary>
    /// Resolve expired subscription ids
    /// </summary>
    public interface ISubscriptionIdResolver
    {
        /// <summary>
        /// You need to replace the old subscription id with the new one, if no new subscription id is provided, just remove the old one from your records.
        /// </summary>
        /// <param name="oldSubscriptionId">Old subscription id</param>
        /// <param name="newSubscriptionId">new subscription id</param>
        Task ResolveExpiredSubscription(string oldSubscriptionId, string newSubscriptionId);
    }

    class NullSubscriptionIdResolver : ISubscriptionIdResolver
    {
        public Task ResolveExpiredSubscription(string oldSubscriptionId, string newSubscriptionId)
        {
            return Task.CompletedTask;
        }
    }
}
