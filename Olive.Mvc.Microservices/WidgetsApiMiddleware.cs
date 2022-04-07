using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Olive.Mvc.Microservices
{
    internal static class WidgetsApiMiddleware
    {
        internal static async Task GetWidgets(HttpContext context)
        {

            var boardWidgets = GetNavigationsFromAssembly<BoardWidgets>();

            if (boardWidgets.None()) return;
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(boardWidgets.Select(x => x.GetWidgets()).SelectMany(x => x).ToList());
            await context.Response.WriteAsync(result);
        }

        private static IEnumerable<T> GetNavigationsFromAssembly<T>() where T : BoardWidgets
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();
            types = types.Where(x => x.IsA<BoardWidgets>() && !x.IsAbstract).ToArray();

            var objects = types.Select(t => (T)Activator.CreateInstance(t)).ToArray();

            foreach (var x in objects)
                x.Define();

            return objects;
        }
    }
