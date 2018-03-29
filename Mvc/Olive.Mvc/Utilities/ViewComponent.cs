namespace Olive.Mvc
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Olive.Entities;

    public abstract class ViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        protected static IDatabase Database => Entities.Data.Database.Instance;

        /// <summary>
        /// Gets HTTP-specific information about an individual HTTP request.
        /// </summary>
        public new HttpContext HttpContext => base.HttpContext ?? Context.Current.Http();

        protected new HttpRequest Request => HttpContext?.Request;

        public ActionResult Redirect(string url) => new RedirectResult(url);

        protected async Task<TViewModel> Bind<TViewModel>(object settings) where TViewModel : IViewModel, new()
        {
            var result = new TViewModel();
            if (settings != null) await ViewModelServices.CopyData(settings, result);
            return result;
        }

        /// <summary>
        /// Loads a Javascript (or Typescript) module upon page startup.
        /// </summary>
        /// <param name="relativePath">The relative path of the module inside wwwroot (including the .js extension).
        /// E.g. /scripts/CustomModule1</param>
        /// <param name="staticFunctionInvokation">An expression to call [a static method] on the loaded module.</param>
        public void LoadJavascriptModule(string relativePath, string staticFunctionInvokation = "run()")
        {
            var onLoaded = staticFunctionInvokation.WithPrefix(", m => m.default.");
            var fullUrl = Request.GetAbsoluteUrl(relativePath);
            Context.Current.Http().JavascriptActions().JavaScript("loadModule('" + fullUrl + "'" + onLoaded + ");");
        }
    }
}