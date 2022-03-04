using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Olive.Mvc.Microservices
{
    internal static class NavigationApiMiddleWare
    {
        internal static async Task Navigate(HttpContext context)
        {

            var navigations = GetNavigationsFromAssembly<Navigation>();

            if (navigations.None()) return;
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(navigations.Select(x => x.GetFeatures()).SelectMany(x => x).ToList());
            await context.Response.WriteAsync(result);
        }

        public static IEnumerable<T> GetNavigationsFromAssembly<T>() where T : Navigation
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();
            types = types.Where(x => x.IsA<Navigation>() && !x.IsAbstract).ToArray();

            var objects = types.Select(t => (T)Activator.CreateInstance(t)).ToArray();

            foreach (var x in objects)
                x.Define();

            return objects;
        }
    }
}
