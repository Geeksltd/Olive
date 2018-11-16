using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ASP
{
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ControlValidationTranslation
    {
        static bool TranslateValidators = Config.Get("Globalization:TranslateValidators", defaultValue: false);
        static string[] ValidationTextAttributes = new[] { "data-val-length", "data-val-required", "data-val-email" };

        internal static IHtmlContent Translate(IHtmlContent source)
        {
            if (!TranslateValidators) return source;

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(source.ToString());

            foreach (var node in doc.DocumentNode.Descendants())
                foreach (var att in node.Attributes.OrEmpty())
                    if (att.Name.IsAnyOf(ValidationTextAttributes))
                        att.Value = Translator.Translate(att.Value);

            return new HtmlString(doc.DocumentNode.InnerHtml);
        }

        public static IHtmlContent TextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var result = InputExtensions.TextBoxFor(htmlHelper, expression, htmlAttributes);

            return Translate(result);
        }

        public static IHtmlContent PasswordFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var result = InputExtensions.PasswordFor(htmlHelper, expression, htmlAttributes);

            return Translate(result);
        }

        public static IHtmlContent RadioButtonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
           Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            var result = InputExtensions.RadioButtonFor(htmlHelper, expression, htmlAttributes);

            return Translate(result);
        }

        public static IHtmlContent CheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper,
           Expression<Func<TModel, bool>> expression, object htmlAttributes = null)
        {
            var result = InputExtensions.CheckBoxFor(htmlHelper, expression, htmlAttributes);

            return Translate(result);
        }
    }
}