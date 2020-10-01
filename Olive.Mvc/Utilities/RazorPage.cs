using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Olive.Entities;
using System.IO;

namespace Olive.Mvc
{
    public abstract class RazorPage<TModel> : Microsoft.AspNetCore.Mvc.Razor.RazorPage<TModel>
    {
#pragma warning disable IDE1006
        /// <summary>
        /// Gets the View Model instance to provide a consistent API to gain access to the ViewModel object from controller and View.
        /// </summary>
        protected virtual TModel info => Model;
#pragma warning restore IDE1006

        public HttpRequest Request => Context.Request;

        protected static IDatabase Database => Olive.Context.Current.Database();

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

        public virtual IWebHostEnvironment Environment => Olive.Context.Current.GetService<IWebHostEnvironment>();
    }
}