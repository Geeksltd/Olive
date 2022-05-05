using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Olive.Mvc.Microservices
{
    internal static class DevelopmentShareInfo
    {
        internal static bool Shared = false;
        private static string[] GetBoardSources(List<Navigation> navigations)
        {
            return navigations
                .SelectMany(x => x.GetType().GetMethods().Where(x => x.Name == "DefineDynamic"))
                .Select(x => x.GetParameters().LastOrDefault().ParameterType.Name)
                .Distinct().ToArray();
        }
        private static Feature[] GetFeatures(List<Navigation> navigations) => navigations.Select(x => x.GetFeatures()).SelectMany(x => x).ToArray();

        private static Service GetService()
        {
            return new Service()
            {
                Name = Microservice.Me.Name,
                BaseUrl = Microservice.Me.Url(),
                Icon = "fas fa-" + Microservice.Me.Name.ToLower(),
            };
        }
        static Type[] AllLoadedTypes() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();
        private static IEnumerable<T> GetControllerFromAssembly<T>() where T : Olive.GlobalSearch.SearchSource
        {
            var types = AllLoadedTypes();
            types = types.Where(x => x.IsA<Olive.GlobalSearch.SearchSource>() && !x.IsAbstract).ToArray();
            return types.Select(t => (T)Activator.CreateInstance(t)).ToArray();
        }
        internal static RequestDelegate ShareMyData(RequestDelegate next)
        {
            return async ctx =>
            {
                var navigations = NavigationApiMiddleWare.GetNavigationsFromAssembly<Navigation>().ToList();
                if (navigations.HasAny())
                {
                    try
                    {
                        var url = Microservice.Of("Hub").Url("local-setup");
                        navigations.Do(r => r.Define());
                        new WebClient().UploadString(url, Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            Service = GetService(),
                            BoardSources = GetBoardSources(navigations),
                            Features = GetFeatures(navigations),
                            GlobalySearchable = GetControllerFromAssembly<Olive.GlobalSearch.SearchSource>().HasAny()
                        }));
                        Shared = true;
                    }
                    catch (Exception ex)
                    {
                        Log.For(typeof(DevelopmentShareInfo)).Error("With URL: " + Microservice.Of("Hub").Url("local-setup") + "\nCould not reach local hub.\n" + ex);
                    }
                }
                await next(ctx);
            };
        }
    }
}
