using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Olive.Mvc.Microservices
{
    public abstract class Navigation
    {
        /// <summary>
        /// Feature{
        ///     FullPath = "Path/TO/The/Page"
        ///     Icon = "fas fa-key"
        ///     RelativeUrl = "url/path/in/the/microservice"
        ///     Permissions = "permission1, permission2"
        ///     Description = "Brief description"
        /// }
        /// </summary>
        public class Feature
        {
            public string FullPath;
            public string Icon;
            public string RelativeUrl;
            public string Permissions;
            public string Description;
        }

        protected List<Feature> Features = new List<Feature>();

        internal List<Feature> GetFeatures() => Features;
        protected static IUrlHelper Url => new UrlHelper(new ActionContext(Context.Current.Http(), new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()));

        public abstract void Define();

        protected void Add<TController>(string fullPath = null, string icon = null, string desc = null) where TController : Controller
        {
            var url = Url.Index<TController>();
            if (fullPath.IsEmpty())
                fullPath = typeof(TController).Name.TrimEnd("Controller");

            var permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(fullPath, url, permissions, icon, desc);
        }

        protected void Add(string fullPath, string url, string permissions, string icon, string desc)
        {
            Add(new Feature
            {
                FullPath = fullPath,
                RelativeUrl = url,
                Permissions = permissions,
                Icon = icon,
                Description = desc
            });
        }

        protected void Add(Feature feature) => Features.Add(feature);

    }
}