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
        public class Feature
        {
            public string Name;
            public string Icon;
            public string Url;
            public string Permissions;
        }

        protected List<Feature> Features = new List<Feature>();

        protected static IUrlHelper Url => new UrlHelper(Context.Current.ActionContext());

        public abstract void Define();

        protected void Add<TController>(string feature = null, string icon = null) where TController : Controller
        {
            var url = Url.Index<TController>();
            if (feature.IsEmpty())
                feature = typeof(TController).Name.TrimEnd("Controller");

            var permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(feature, url, permissions, icon);
        }

        protected void Add(string feature, string url, string permissions, string icon)
        {
            Add(new Feature
            {
                Name = feature,
                Url = url,
                Permissions = permissions,
                Icon = icon
            });
        }

        protected void Add(Feature feature) => Features.Add(feature);
    }
}