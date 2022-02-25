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
            public string Featues;
            public string Icon;
            public string Url;
            public string Permissions;
            public string Desc;
        }

        protected List<Feature> Features = new List<Feature>();

        internal List<Feature> GetFeatures() => Features;
        protected static IUrlHelper Url => new UrlHelper(Context.Current.ActionContext());

        public abstract void Define();

        protected void Add<TController>(string features = null, string icon = null, string desc = null) where TController : Controller
        {
            var url = Url.Index<TController>();
            if (features.IsEmpty())
                features = typeof(TController).Name.TrimEnd("Controller");

            var permissions = typeof(TController).GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            Add(features, url, permissions, icon, desc);
        }

        protected void Add(string features, string url, string permissions, string icon, string desc)
        {
            Add(new Feature
            {
                Featues = features,
                Url = url,
                Permissions = permissions,
                Icon = icon,
                Desc = desc
            });
        }

        protected void Add(Feature feature) => Features.Add(feature);
    }
}