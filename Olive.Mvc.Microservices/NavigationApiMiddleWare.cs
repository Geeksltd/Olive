using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;

namespace Olive.Mvc.Microservices
{
    internal static class NavigationApiMiddleWare
    {
        internal static async Task Navigate(HttpContext context)
        {
            var navigations = GetNavigationsFromAssembly<Navigation>();
            navigations.Do(r => r.Define());

            if (navigations.None()) return;
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(navigations.Select(x => x.GetFeatures()).SelectMany(x => x).ToList());
            await context.Response.WriteAsync(result);
        }

        static Dictionary<string, Type> BoardTypeCache = new Dictionary<string, Type>();

        static Func<Type> DiscoverType(string name) => AllLoadedTypes().FirstOrDefault(x => x.Name == name).GetType;
        internal static async Task Search(HttpContext context)
        {
            var id = context.Request.Param("boardItemId").OrEmpty();
            var typeName = context.Request.Param("boardtype").OrEmpty();
            if (id.IsEmpty() || typeName.IsEmpty()) return;
            var type = BoardTypeCache.GetOrAdd(typeName, DiscoverType(typeName));
            if (type == null) return;
            var navigations = GetNavigationsFromAssembly<Navigation>().ToList();
            foreach (var nav in navigations)
            {
                var guideEntity = id.Is<Guid>() ? (GuidEntity)await Context.Current.Database().Get(id.To<Guid>(), type) : await nav.GetBoardObjectFromText(type, id);
                nav.DefineDynamic(context.User, guideEntity);
            }
            var response = Newtonsoft.Json.JsonConvert.SerializeObject(
                new
                {
                    Widgets = navigations.SelectMany(x => x.GetBoardwidgets()),
                    Htmls = navigations.SelectMany(x => x.GetBoardHtmls()),
                    Buttons = navigations.SelectMany(x => x.GetBoardButtons()),
                    Infos = navigations.SelectMany(x => x.GetBoardInfos()),
                    Menues = navigations.SelectMany(x => x.GetBoardMenues()),
                });
            await context.Response.WriteAsync(response);
        }

        static IEnumerable<T> GetNavigationsFromAssembly<T>() where T : Navigation
        {
            var types = AllLoadedTypes();
            types = types.Where(x => x.IsA<Navigation>() && !x.IsAbstract).ToArray();
            return types.Select(t => (T)Activator.CreateInstance(t)).ToArray();
        }

        static Type[] AllLoadedTypes() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();
    }
}