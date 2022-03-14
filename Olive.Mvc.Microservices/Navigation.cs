using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

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
    }
    public abstract class Navigation
    {
        protected List<Feature> Features = new List<Feature>();

        internal List<Feature> GetFeatures() => Features;
        protected static IUrlHelper Url => new UrlHelper(new ActionContext(Context.Current.Http(), new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        public abstract void Define();

        protected void Add<TController>(string fullPath = null, string icon = null, string desc = null, string @ref = null, string badgeUrl = null, bool showOnRight = false, bool iframe = false) where TController : Controller
        {
            var url = Url.Index<TController>();
            if (fullPath.IsEmpty())
                fullPath = typeof(TController).Name.TrimEnd("Controller");

            var permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(fullPath, url, permissions, icon, desc, @ref, badgeUrl, showOnRight, iframe);
        }

        protected void Add(string fullPath, string url, string permissions, string icon, string desc, string @ref, string badgeUrl, bool showOnRigh, bool iframe)
        {
            Add(new Feature
            {
                FullPath = fullPath,
                RelativeUrl = url,
                Permissions = permissions,
                Icon = icon,
                Description = desc,
                Refrance = @ref,
                BadgeUrl = badgeUrl,
                ShowOnRight = showOnRigh,
                Iframe = iframe
            });
        }

        protected void Add(Feature feature) => Features.Add(feature);

    }
}