using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Olive.Mvc.Microservices
{
    internal static class MenuApiMiddleWare
    {
        internal static async Task Menu(HttpContext context)
        {
            var files = AppDomain.CurrentDomain.WebsiteRoot().GetFiles("Features*.xml");

            if (files.None())
                return;

            var data = files.Select(x => ReadXml(x.Name)).SelectMany(x => x).Select(x => x.ToString()).Aggregate((current, next) => $"{current} {next}");

            var result = Security.Encryption.Encrypt(data.WithWrappers("<ROOT>", "</ROOT>"), Config.Get("DataEncryption:Menu", "Hub.Menu"));

            await context.Response.WriteAsync(result);
        }

        static IEnumerable<XElement> ReadXml(string file)
        {
            return AppDomain.CurrentDomain.WebsiteRoot().GetFile(file)
                .ReadAllText().To<XDocument>().Root.Elements();
        }
    }
}
