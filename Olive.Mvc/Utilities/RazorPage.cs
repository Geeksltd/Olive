using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Olive.Services.Globalization;

namespace Olive.Mvc
{
    public abstract class RazorPage<TModel> : Microsoft.AspNetCore.Mvc.Razor.RazorPage<TModel>
    {
        /// <summary>
        /// Gets the View Model instance to provide a consistent API to gain access to the ViewModel object from controller and View.
        /// </summary>
        protected virtual TModel info => Model;

        public HttpRequest Request => Context.Request;

        /// <summary>
        /// Will return the translation of the specified phrase in the language specified in user's cookie (or default language).
        /// </summary>
        public static Task<string> Translate(string phrase) => Translator.Translate(phrase);

        /// <summary>
        /// Will return the translation of the specified markup in the language specified in user's cookie (or default language).
        /// </summary>
        public static async Task<HtmlString> TranslateHtml(string markup)
        {
            if (markup.IsEmpty()) return HtmlString.Empty;

            return new HtmlString(await Translator.TranslateHtml(markup));
        }

        /// <summary>
        /// Gets a file from its relative path.
        /// </summary>
        public FileInfo MapPath(string relativePath)
        {
            var fileProvider = Environment.ContentRootFileProvider;
            var path = fileProvider.GetFileInfo(relativePath)?.PhysicalPath;
            if (path.IsEmpty()) return null;
            return path.AsFile();
        }

        public virtual IHostingEnvironment Environment => Web.Context.HostingEnvironment;
    }
}