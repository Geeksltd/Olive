using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Services.PushNotification
{
    public interface IUserDevice
    {
        /// <summary>
        /// IOS, Android or Windows.
        /// </summary>
        string DeviceType { get; }

        /// <summary>
        ///  Push notification token registered with the platform service.
        /// </summary>
        string PushNotificationToken { get; }
    }
}
