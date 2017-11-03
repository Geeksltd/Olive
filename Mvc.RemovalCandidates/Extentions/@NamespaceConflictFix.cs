using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace ASP
{
    /// <summary>
    /// Defined in the ASP namespace to fix the namespece conflicts for extension methods with the same name.
    /// These methods are preferred by the compiler for all code written in CSHTML files.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SystemMvcWebExtensionsFix
    {
        delegate IHtmlContent SelectControlHelper(HtmlHelper helper, string name, IEnumerable<SelectListItem> selectList, object htmlAttributes);

        delegate IHtmlContent SelectControlHelper<TModel, TProperty>(HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes);

        delegate IHtmlContent InputControlHelper(HtmlHelper helper, string name, string value, object htmlAttributes);

        delegate IHtmlContent InputControlHelper<TModel, TProperty>(HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes);

        /// <summary>
        /// Will replace all line breaks with a BR tag and return the result as a raw html.
        /// </summary>
        public static IHtmlContent ToHtmlLines(this string text) => new HtmlString(Olive.OliveExtensions.ToHtmlLines(text));

        /// <summary>
        /// Will join all items with a BR tag and return the result as a raw html.
        /// </summary>
        public static IHtmlContent ToHtmlLines(this IEnumerable<string> items) => new HtmlString(Olive.OliveExtensions.ToHtmlLines(items));

        /// <summary>
        /// Will join all items with a BR tag and return the result as a raw html.
        /// </summary>
        public static IHtmlContent ToHtmlLines<T>(this IEnumerable<T> items) => new HtmlString(Olive.OliveExtensions.ToHtmlLines(items));

        /// <summary>
        /// Determines whether this text is null or empty.
        /// </summary>
        public static bool IsEmpty(this string text) => string.IsNullOrEmpty(text);

        /// <summary>
        /// Returns an HTML select element for each property in the object that is represented by the specified expression 
        /// using the specified list items and HTML attributes.
        /// </summary>
        public static IHtmlContent DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            return FixForEntityType(htmlHelper, expression, selectList, htmlAttributes, SelectExtensions.DropDownList,
                SelectExtensions.DropDownListFor);
        }

        public static IHtmlContent HiddenFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            return FixForEntityType(htmlHelper, expression, htmlAttributes, InputExtensions.Hidden, InputExtensions.HiddenFor);
        }

        static IHtmlContent FixForEntityType<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, InputControlHelper controlMethod, InputControlHelper<TModel, TProperty> generic)
        {
            var fixedCode = FixForEntityType<TModel, TProperty>(htmlHelper, expression, htmlAttributes, controlMethod);

            if (fixedCode != null) return fixedCode;

            return generic(htmlHelper, expression, htmlAttributes);
        }

        static IHtmlContent FixForEntityType<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, SelectControlHelper controlMethod, SelectControlHelper<TModel, TProperty> generic)
        {
            var fixedCode = FixForEntityType<TModel, TProperty>(htmlHelper, expression, selectList, htmlAttributes, controlMethod);

            if (fixedCode != null) return fixedCode;

            return ControlValidationTranslation.Translate(generic(htmlHelper, expression, selectList, htmlAttributes));
        }

        static IHtmlContent FixForEntityType<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, SelectControlHelper controlMethod)
        {
            var property = expression.GetProperty();

            if (property == null) return null;

            if (!property.PropertyType.IsA<IEntity>()) return null;

            var value = property.GetValue(htmlHelper.ViewData.Model) as IEntity;
            if (value == null) return null;

            htmlHelper.ViewContext.ViewData[property.Name] = value.GetId().ToString();

            return ControlValidationTranslation.Translate(controlMethod(htmlHelper, property.Name, selectList, htmlAttributes));
        }

        static IHtmlContent FixForEntityType<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, InputControlHelper controlMethod)
        {
            var property = expression.GetProperty();

            if (property == null) return null;

            string value;

            var type = property.PropertyType;

            if (type.IsA<IEntity>())
            {
                var entity = property.GetValue(htmlHelper.ViewData.Model) as IEntity;
                if (entity == null) return null;
                value = entity.GetId().ToString();
            }
            else if (type.IsA<IEnumerable>() && type.GetGenericArguments().FirstOrDefault()?.IsA<IEntity>() == true)
            {
                var list = property.GetValue(htmlHelper.ViewData.Model) as IEnumerable;
                if (list == null) return null;
                value = list.Cast<IEntity>().Select(x => x.GetId()).ToString("|");
            }
            else return null;

            return ControlValidationTranslation.Translate(controlMethod(htmlHelper, property.Name, value, htmlAttributes));
        }
    }
}