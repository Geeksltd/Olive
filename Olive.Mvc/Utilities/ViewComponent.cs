namespace Olive.Mvc
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Olive.Entities;
    using Olive.Web;

    public abstract class ViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        protected static IDatabase Database => Entities.Data.Database.Instance;

        /// <summary>
        /// Gets HTTP-specific information about an individual HTTP request.
        /// </summary>
        public new HttpContext HttpContext => base.HttpContext ?? Context.Http;

        protected new HttpRequest Request => HttpContext?.Request;

        public ActionResult Redirect(string url) => new RedirectResult(url);
    }
}