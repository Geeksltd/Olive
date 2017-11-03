using System;

namespace Olive.Mvc
{
    partial class MSharpMvcExtensions
    {
        // *****************************************************************
        // The following methods remove because in .net core the request
        // context does not accessible from the htmlHelper and ...
        // *****************************************************************
        public static UrlHelper GetUrlHelper(this HtmlHelper html) => new UrlHelper(html.ViewContext.RequestContext);

        // *****************************************************************
        // The Following method adds overload to existing methods in the old
        // .net and in the core version these methods has been removed.
        // *****************************************************************

        /// <summary>
        /// Renders the specified partial view as an HTML-encoded string.
        /// </summary>
        /// <param name="html">The HTML helper instance that this method extends.</param>
        /// <param name="partialViewName">The name of the partial view to render.</param>
        /// <param name="model">The view model for the partial view.</param>
        /// <returns>The partial view that is rendered as an HTML-encoded string.</returns>
        public static HtmlString Partial<T>(this HtmlHelper html, string partialViewName, T model, bool skipAjaxPost) where T : IViewModel
        {
            var request = HttpContext.Current.Request;
            if (skipAjaxPost && request.IsAjaxCall() && request.IsPost()) return HtmlString.Empty;

            if (model == null)
            {
                model = (html.ViewContext.Controller as Controller).Bind<T>();

                if (model == null)
                    throw new Exception("The model object passed to Partial() cannot be null.");
            }

            return html.Partial(partialViewName, model);
        }

        public static void RenderAction<TController>(this HtmlHelper html, string action = "Index")
        {
            html.RenderAction(action, typeof(TController).Name.TrimEnd("Controller"));
        }

        /// <summary>
        /// Invokes the Index action method of the specified controller and returns the result as an HTML string.
        /// <param name="queryParameters">An anonymous object containing query string / route values to pass.</param>
        /// </summary>
        public static HtmlString Action<TController>(this HtmlHelper html, object queryParameters)
        {
            return Action<TController>(html, "Index", queryParameters);
        }

        /// <summary>
        /// Invokes the specified child action method of the specified controller and returns the result as an HTML string.
        /// <param name="queryParameters">An anonymous object containing query string / route values to pass.</param>
        /// </summary>
        public static HtmlString Action<TController>(this HtmlHelper html, string action = "Index", object queryParameters = null)
        {
            return html.Action(action, typeof(TController).Name.TrimEnd("Controller"), queryParameters);
        }
    }
}