namespace Olive.Mvc.Microservices
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Security.Claims;

    internal static class BoardApiMiddleware
    {
        internal static async Task Search(HttpContext context)
        {
            var id = context.Request.Param("boardItemId").OrEmpty();
            var type = context.Request.Param("boardtype").OrEmpty();
            if (id.IsEmpty() || type.IsEmpty()) return;

            var boardInstances = await GetNavigationsFromAssembly<BoardComponentsSource>(context.User, id, type);
            var response = JsonConvert.SerializeObject(new { Results = boardInstances.SelectMany(x => x.Results), AddabledItems = boardInstances.SelectMany(x => x.AddableItems) });
            await context.Response.WriteAsync(response);
        }
        private static async Task<IEnumerable<T>> GetNavigationsFromAssembly<T>(ClaimsPrincipal user, string id, string type) where T : BoardComponentsSource
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray();
            types = types.Where(x => x.IsA<BoardComponentsSource>() && !x.IsAbstract).ToArray();

            var objects = types.Select(t => (T)Activator.CreateInstance(t)).ToArray();

            foreach (var x in objects)
                await x.Process(user, id, type);

            return objects;
        }
    }
}
