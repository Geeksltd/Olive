using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;

namespace Olive.Mvc.Microservices
{
    internal static class NavigationApiMiddleWare
    {
        static Dictionary<string, Type> BoardTypeCache = new Dictionary<string, Type>();

        internal static async Task Navigate(HttpContext context)
        {
            var navigations = GetNavigationsFromAssembly<Navigation>();
            navigations.Do(r => r.Define());

            if (navigations.None()) return;
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(navigations.Select(x => x.GetFeatures()).SelectMany(x => x).ToList());
            await context.Response.WriteAsync(result);
        }

        static Type DiscoverType(string name) => AllLoadedTypes().FirstOrDefault(x => x.IsA<GuidEntity>() && x.Name == name);
        internal static async Task Search(HttpContext context)
        {
            var id = context.Request.Param("id").OrEmpty();
            var typeName = context.Request.Param("type").OrEmpty();
            if (id.IsEmpty() || typeName.IsEmpty()) return;
            Type type;
            if (!BoardTypeCache.TryGetValue(typeName, out type))
            {
                type = DiscoverType(typeName);
                BoardTypeCache.Add(typeName, type);
            }
            if (type == null) return;
            var navigations = GetNavigationsFromAssembly<Navigation>().ToList();
            foreach (var nav in navigations)
            {
                foreach (var defineDynamic in nav.GetType().GetMethods().Where(x => x.Name == "DefineDynamic"))
                {
                    object secondParameter = null;
                    if (id.Is<Guid>())
                        secondParameter = await Context.Current.Database().Get(id.To<Guid>(), type);
                    else
                        secondParameter = await nav.GetBoardObjectFromText(type, id);
                    if (secondParameter == null) continue;
                    try
                    {
                        await (Task)defineDynamic.Invoke(nav, new object[] { context.User, Convert.ChangeType(secondParameter, type) });
                    }
                    catch (Exception ex)
                    {
                        Log.For(defineDynamic).Info("faild to invode define dynamic:" + ex);
                    }
                }
            }

            var response = Newtonsoft.Json.JsonConvert.SerializeObject(
                new
                {
                    Widgets = navigations.SelectMany(x => x.GetBoardwidgets()),
                    Htmls = navigations.SelectMany(x => x.GetBoardHtmls()),
                    Buttons = navigations.SelectMany(x => x.GetBoardButtons()),
                    Infos = navigations.SelectMany(x => x.GetBoardInfos()),
                    Menus = navigations.SelectMany(x => x.GetBoardMenus()),
                    Intros = navigations.SelectMany(x => x.GetBoardIntros()),
                });
            await context.Response.WriteAsync(response);
        }
        internal static IEnumerable<T> GetNavigationsFromAssembly<T>() where T : Navigation
        {
            var types = AllLoadedTypes();
            types = types.Where(x => x.IsA<Navigation>() && !x.IsAbstract).ToArray();
            return types.Select(t => (T)Activator.CreateInstance(t)).ToArray();
        }
        internal static async Task BoardSources(HttpContext context)
        {
            var navigations = GetNavigationsFromAssembly<Navigation>();

            if (navigations.None()) return;
            var boardSources = navigations
                .SelectMany(x => x.GetType().GetMethods().Where(x => x.Name == "DefineDynamic"))
                .Select(x => x.GetParameters().LastOrDefault().ParameterType.Name).Distinct();
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(boardSources);
            await context.Response.WriteAsync(result);
        }
        static Type[] AllLoadedTypes() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();
    }
}