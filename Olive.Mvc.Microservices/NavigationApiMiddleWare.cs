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
            var navigations = GetEnumerableOfType<Navigation>();

            if (navigations.None()) return;
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(navigations.Select(x => x.GetFeatures()).SelectMany(x => x).ToList());
            await context.Response.WriteAsync(result);
        }
        public static IEnumerable<T> GetEnumerableOfType<T>() where T : Navigation
        {
            var objects = new List<T>();
            foreach (var type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type));
            }
            objects.ForEach(x => x.Define());
            return objects;
        }
    }
}
