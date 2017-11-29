using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis;
using Olive.Entities;
using Olive.Web;

namespace Olive.Mvc
{
    /// <summary>
    /// Provides methods that respond to HTTP requests that are made to an ASP.NET MVC Web site.
    /// </summary>
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public abstract partial class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        /// <summary>
        /// Initializes a new instance of the System.Web.Mvc.Controller class.
        /// </summary>
        protected Controller()
        {
            ModelState.Clear();
            HttpContext.Items["ViewBag"] = ViewBag;
        }

        protected static IDatabase Database => Entities.Data.Database.Instance;

        /// <summary>
        /// Provides a dummy Html helper.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public HtmlHelper Html
        {
            get
            {
                throw new NotImplementedException("The following code is commented to fix on the test time.");
                // return new HtmlHelper(new ViewContext(ControllerContext,
                // 	new RazorView(ControllerContext, "unknownPath", "unknownLayout", false, null),
                // 	new ViewDataDictionary(),
                // 	new TempDataDictionary(), TextWriter.Null), new ViewPage());
            }
        }

        /// <summary>
        /// Do not use this overload. Always provide a viewmodel as a parameter.
        /// </summary>
        protected new internal ViewResult View() =>
            throw new InvalidOperationException("View() method should not be called without specifying a view model.");

        /// <summary>
        /// Creates a ViewResult object by using the model that renders a view to the response.
        /// </summary>
        protected new internal async Task<ViewResult> View(object model)
        {
            AddAction(await NotificationAction.GetScheduledNotification());
            return base.View(model);
        }

        /// <summary>
        /// Creates a ViewResult object by using the model that renders a view to the response.
        /// </summary>
        protected new internal async Task<ViewResult> View(string viewName)
        {
            AddAction(await NotificationAction.GetScheduledNotification());
            return base.View(viewName);
        }

        /// <summary>
        /// Creates a ViewResult object by using the model that renders a view to the response.
        /// </summary>
        protected new internal async Task<ViewResult> View(string viewName, object model)
        {
            AddAction(await NotificationAction.GetScheduledNotification());
            return base.View(viewName, model);
        }

        /// <summary>
        /// Gets HTTP-specific information about an individual HTTP request.
        /// </summary>
        public new HttpContext HttpContext => base.HttpContext ?? Context.Http;

        /// <summary>
        /// Creates a new instance of the specified view model type and binds it using the standard request data.
        /// </summary>

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await BindAttributeRunner.Run(context, next);
            await base.OnActionExecutionAsync(context, next);
        }

        public override async Task<bool> TryUpdateModelAsync<TModel>(TModel model)
        {
            if (await base.TryUpdateModelAsync(model))
            {
                await BindAttributeRunner.BindOn(this, (IViewModel)model);
                return true;
            }

            return false;
        }

        protected new HttpRequest Request => HttpContext.Request;

        protected List<object> Actions =>
            (List<object>)(HttpContext.Items["JavascriptActions"] ?? (HttpContext.Items["JavascriptActions"] = new List<object>()));

        [NonAction]
        protected JsonResult AddAction(object action)
        {
            if (action != null) Actions.Add(action);

            return JsonActions();
        }

        [NonAction]
        protected JsonResult JsonActions() => Json(Actions);

        [NonAction]
        protected async Task<ActionResult> JsonActions(IViewModel info)
        {
            if (Request.IsAjaxCall()) return Json(Actions);
            else return await View(info);
        }

        [NonAction]
        protected JsonResult Notify(object message, bool obstruct = true) => Notify(message, style: null, obstruct: obstruct);

        [NonAction]
        protected JsonResult Notify(object message, string style, bool obstruct = true) =>
            AddAction(new NotificationAction { Notify = message.ToStringOrEmpty(), Style = style, Obstruct = obstruct });

        [NonAction]
        public JsonResult JavaScript(string script) => JavaScript(script, PageLifecycleStage.Init);

        [NonAction]
        public JsonResult JavaScript(string script, PageLifecycleStage stage) =>
            AddAction(new { Script = script, Stage = stage.ToString() });

        [NonAction]
        public ActionResult AjaxRedirect(string url)
        {
            url = url.Or("#");
            if (!url.OrEmpty().ToLower().StartsWithAny("/", "http:", "https:")) url = "/" + url;

            if (Actions.OfType<NotificationAction>().Any())
                NotificationAction.ScheduleForNextRequest(Actions);

            Actions.Add(new { Redirect = url, WithAjax = true });

            return JsonActions();
        }

        [NonAction]
        public ActionResult Redirect(string url, string target = null)
        {
            url = url.Or("#");
            if (!url.OrEmpty().ToLower().StartsWithAny("/", "http:", "https:")) url = "/" + url;

            if (Actions.OfType<NotificationAction>().Any())
                NotificationAction.ScheduleForNextRequest(Actions);

            if (Request.IsAjaxCall() || target.HasValue())
            {
                Actions.Add(new { Redirect = url, Target = target });

                return JsonActions();
            }
            else
            {
                return base.Redirect(url);
            }
        }

        [NonAction]
        protected JsonResult Do(WindowAction action)
        {
            if (Actions.OfType<NotificationAction>().Any())
                if (new[] { WindowAction.Refresh, WindowAction.CloseModalRefreshParent }.Contains(action))
                    NotificationAction.ScheduleForNextRequest(Actions);

            return AddAction(new { BrowserAction = action.ToString() });
        }

        [NonAction]
        protected JsonResult ReplaceView(string text, bool htmlEncode = true)
        {
            if (htmlEncode) text = text.HtmlEncode();

            return AddAction(new { ReplaceView = text });
        }

        [NonAction]
        protected JsonResult ReplaceSource<T>(string controlId, IEnumerable<SelectListItem> newItems) =>
            AddAction(new { ReplaceSource = controlId, Items = newItems.ToList() });

        [NonAction]
        protected ActionResult SubFormView<TViewModel>(string subFormName, string subFormView) where TViewModel : new() =>
            SubFormView(subFormName, subFormView, new TViewModel());

        [NonAction]
        protected JsonResult SubFormView(string subFormName, string subFormView, object viewModel)
        {
            ModelState.Clear();

            var view = RenderPartialView(subFormView, viewModel);

            return AddAction(new { SubForm = subFormName, NewItem = view });
        }

        protected async Task<string> RenderPartialView(string viewName, object model)
        {
            var renderer = HttpContext.RequestServices.GetService(typeof(IViewRenderService)) as IViewRenderService;

            return await renderer.RenderToStringAsync(viewName, model);
        }

        protected virtual ActionResult RedirectToLogin() =>
            Redirect("/login?ReturnUrl=" + HttpContext.GetUrlHelper().Current().UrlEncode());
    }

    public enum WindowAction
    {
        CloseModal,
        CloseModalRefreshParent,
        Refresh,
        Close,
        Back,
        Print,
        ShowPleaseWait
    }
}