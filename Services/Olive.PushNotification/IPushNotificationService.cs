using System.Collections.Generic;

namespace Olive.PushNotification
{
    /// <summary>
    /// Sends push notifications to ios, android and windows devices
    /// </summary>
    public interface IPushNotificationService
    {
        bool Send(string messageTitle, string messageBody, IEnumerable<IUserDevice> devices);

        /// <summary>
        /// Updates badge number of user's devices
        /// </summary>
        /// <param name="badge">0 for removing badge icon</param>
        bool UpdateBadge(int badge, IEnumerable<IUserDevice> devices);
    }
}
