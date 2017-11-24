namespace Olive.Mvc
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Olive.Entities;
    using Olive.Services.Globalization;
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

        /// <summary>
        /// Will return the translation of the specified phrase in the language specified in user's cookie (or default language).
        /// </summary>
        public static Task<string> Translate(string phrase) => Translator.Translate(phrase);

        /// <summary>
        /// Will return the translation of the specified validation exception's message in the language specified in user's cookie (or default language).
        /// If the IsMessageTranslated property is set, it will return message without extra translation.
        /// </summary>
        public static async Task<string> Translate(ValidationException exception)
        {
            if (exception.IsMessageTranslated) return exception.Message;
            else return await Translate(exception.Message);
        }

        /// <summary>
        /// Will return the translation of the specified markup in the language specified in user's cookie (or default language).
        /// </summary>
        public static Task<string> TranslateHtml(string markup) => Translator.TranslateHtml(markup);
    }
}