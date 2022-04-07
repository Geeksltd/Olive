using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc.Microservices
{

    /// <summary>
    /// Represents a single widget that is displayed to the user.
    /// </summary>
    public class Widget
    {
        /// <summary>
        /// Url to which the user will be redirected for adding a new item.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string AddUrl;
        /// <summary>
        /// Url to which the user will be redirected to manage these objects.
        /// For relative Url to the current site use ~/my-url syntax.
        /// </summary>
        public string ManageUrl;
        /// <summary>
        /// Name of the Widget. This is mandatory.
        /// </summary>
        public string Name;
        /// <summary>
        /// colour of the widget
        /// </summary>
        public string Colour;
        /// <summary>
        /// Permissions for acceess management
        /// </summary>
        public string Permissions;
        /// <summary>
        /// Group of the widget EG: People, Projects
        /// </summary>
        public string Group;
    }
    public abstract class BoardWidgets
    {
        protected List<Widget> Widgets = new List<Widget>();

        internal List<Widget> GetWidgets() => Widgets;

        public abstract void Define();
        protected void Add(Widget result)
        {
            if (result == null) return;

            if (result.AddUrl.IsEmpty() && result.ManageUrl.IsEmpty())
                throw new ArgumentException("At least one of AddUrl and ManagerUrl must be provided.");

            if (result.Name.IsEmpty())
                throw new ArgumentException("Name cannot be empty in a widget.");

            result.AddUrl = FixUrl(result.AddUrl);
            result.ManageUrl = FixUrl(result.ManageUrl);
            Widgets.Add(result);
        }
        static string FixUrl(string url)
        {
            if (url.OrEmpty().StartsWith("~/")) return MakeAbsolute(url.TrimStart('~'));
            else return url;
        }
        static string MakeAbsolute(string url) => Context.Current.Request().GetAbsoluteUrl(url);
    }
}
