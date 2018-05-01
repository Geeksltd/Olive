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
using Microsoft.Extensions.Logging;
using Olive.Entities;

namespace Olive.Mvc
{
    /// <summary>
    /// Provides methods that respond to HTTP requests that are made to an ASP.NET MVC Web site.
    /// </summary>
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public abstract partial class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        static bool IsFirstRequest = true;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static event Action OnFirstRequest;

        /// <summary>
        /// Initializes a new instance of the System.Web.Mvc.Controller class.
        /// </summary>
        protected Controller()
        {
            if (IsFirstRequest)
            {
                IsFirstRequest = false;
                try
                {
                    OnFirstRequest?.Invoke();
                }
                catch
                {
                    IsFirstRequest = true;
                    throw;
                }
            }

            ModelState.Clear();
            HttpContext.Items["ViewBag"] = ViewBag;
        }

        protected static IDatabase Database => Context.Current.Database();

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

        JavascriptActions JavascriptActions => HttpContext.JavascriptActions();

        /// <summary>
        /// Do not use this overload. Always provide a viewmodel as a parameter.
        /// </summary>
        protected new internal ViewResult View()
        {
            throw new InvalidOperationException("View() method should not be called without specifying a view model.");
        }

        /// <summary>
        /// Creates a ViewResult object by using the model that renders a view to the response.
        /// </summary>
        public override ViewResult View(string viewName)
        {
            JavascriptActions.ScheduleNotifications();
            return base.View(viewName);
        }

        /// <summary>
        /// Creates a ViewResult object by using the model that renders a view to the response.
        /// </summary>
        public override ViewResult View(string viewName, object model)
        {
            JavascriptActions.ScheduleNotifications();
            return base.View(viewName, model);
        }

        /// <summary>
        /// Gets HTTP-specific information about an individual HTTP request.
        /// </summary>
        public new HttpContext HttpContext => base.HttpContext ?? Context.Current.Http();

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

        [NonAction]
        protected JsonResult AddAction(object action)
        {
            JavascriptActions.Add(action);
            return JsonActions();
        }

        [NonAction]
        protected JsonResult JsonActions() => Json(JavascriptActions);

        [NonAction]
        public async Task<JsonResult> Json<TResult>(Task<TResult> data)
        {
            var result = await data;
            return base.Json(result);
        }

        [NonAction]
        protected ActionResult JsonActions(IViewModel info)
        {
            if (Request.IsAjaxCall()) return JsonActions();
            else return View(info);
        }

        [NonAction]
        protected JsonResult Notify(object message, bool obstruct = true) => Notify(message, style: null, obstruct: obstruct);

        [NonAction]
        protected JsonResult Notify(object message, string style, bool obstruct = true)
        {
            return AddAction(new NotificationAction { Notify = message.ToStringOrEmpty(), Style = style, Obstruct = obstruct });
        }

        [NonAction]
        public JsonResult JavaScript(string script, PageLifecycleStage stage = PageLifecycleStage.Init)
        {
            JavascriptActions.JavaScript(script, stage);
            return JsonActions();
        }

        [NonAction]
        public ActionResult AjaxRedirect(string url)
        {
            JavascriptActions.Redirect(url, withAjax: true);
            return JsonActions();
        }

        [NonAction]
        public ActionResult Redirect(string url, string target = null)
        {
            JavascriptActions.Redirect(url, target: target);

            if (Request.IsAjaxCall() || target.HasValue())
            {
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
            JavascriptActions.Do(action);
            return JsonActions();
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

        [NonAction]
        public NotFoundTextActionResult NotFound(string message) => new NotFoundTextActionResult(message);

        [NonAction]
        public UnauthorizedTextActionResult Unauthorized(string message) => new UnauthorizedTextActionResult(message);

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
            JavaScript("loadModule('" + fullUrl + "'" + onLoaded + ");");
        }

        public ILogger Log => Olive.Log.For(this);
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