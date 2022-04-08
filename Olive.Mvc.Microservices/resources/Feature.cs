using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{
    /// <summary>
    /// Feature{
    ///     FullPath = "Path/TO/The/Page",
    ///     Icon = "fas fa-key",
    ///     RelativeUrl = "url/path/in/the/microservice",
    ///     Permissions = "permission1, permission2",
    ///     Description = "Brief description",
    ///     Refrance    = "Unique refrance for special cases",
    ///     BadgeUrl    = "/@Services/Badge.ashx?type=forecast",
    ///     ShowOnRight = true,
    ///     Iframe      = false
    /// }
    /// </summary>
    public class Feature
    {
        /// <summary>
        /// Folders in Hub navigation seperated by "/" and the name of the feature shown in Hub.
        /// </summary>
        public string FullPath;
        /// <summary>
        /// Icon of the feature shown in hub.
        /// </summary>
        public string Icon;
        /// <summary>
        /// Relative Url of the feature inside the microservice
        /// </summary>
        public string RelativeUrl;
        /// <summary>
        /// Permissions required to access this feature seperated by ", ".
        /// </summary>
        public string Permissions;
        /// <summary>
        /// Description to be shown under the feature in hub view.
        /// </summary>
        public string Description;
        /// <summary>
        /// Refrance for legacy widgets
        /// </summary>
        public string Refrance;
        /// <summary>
        /// badge reletive Url to be shown in hub.
        /// </summary>
        public string BadgeUrl;
        /// <summary>
        /// For legacy board components to be shown on wright 
        /// </summary>
        public bool ShowOnRight;
        /// <summary>
        /// For old microservices to get rendered as Iframes.
        /// </summary>
        public bool Iframe;
        /// <summary>
        /// Detemines the order of features and folders under hub.
        /// the lower the number the higher the feature.
        /// default number for thisone is 100.
        /// </summary>
        public int? Order;
    }
}
