using Microsoft.AspNetCore.Mvc;
using Olive.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.Mvc
{
    class RouteTemplates
    {
        static ConcurrentDictionary<string, RouteTemplate[]> IndexActionRoutes =
         new ConcurrentDictionary<string, RouteTemplate[]>();

        internal static RouteTemplate[] GetTemplates(string controllerName)
        {
            return IndexActionRoutes.GetOrAdd(controllerName, FindTemplates);
        }

        static RouteTemplate[] FindTemplates(string controllerName)
        {
            var relevantAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.References(Assembly.GetExecutingAssembly())).ToList();

            var types = relevantAssemblies.SelectMany(a => a.GetTypes().Where(t => t.Name == controllerName))
                         .ExceptNull()
                         .Where(x => x.InhritsFrom(typeof(ControllerBase))).ToList();

            if (types.None())
                throw new Exception("Controller class not found: " + controllerName);

            if (types.HasMany())
                throw new Exception("Multiple Controller classes found: " + types.Select(x => x.AssemblyQualifiedName).ToLinesString());

            var type = types.Single();

            var indexAction = type.GetMethods().Where(x => x.Name == "Index").ToList();

            if (indexAction.None()) throw new Exception(type.FullName + " has no Index method.");

            if (indexAction.HasMany()) throw new Exception(type.FullName + " has multiple Index methods.");

            var templates = GetRouteTemplates(indexAction.First());

            if (templates.None())
                throw new Exception(type.FullName + ".Index() has no [Route] or [HttpGet] attribute.");

            return templates.Select(x => new RouteTemplate(x)).ToArray();
        }

        static string[] GetRouteTemplates(MethodInfo indexAction)
        {
            return
            indexAction.GetCustomAttributes<RouteAttribute>().Select(x => x.Template)
            .Concat(indexAction.GetCustomAttributes<HttpGetAttribute>().Select(x => x.Template))
            .Trim().Distinct().ToArray();
        }
    }
}
