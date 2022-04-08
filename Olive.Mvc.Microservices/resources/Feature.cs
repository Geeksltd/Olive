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
        public string FullPath;
        public string Icon;
        public string RelativeUrl;
        public string Permissions;
        public string Description;
        public string Refrance;
        public string BadgeUrl;
        public bool ShowOnRight;
        public bool Iframe;
        public int? Order;
    }
}
