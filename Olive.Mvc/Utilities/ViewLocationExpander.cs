using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;

namespace Olive.Mvc
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        /// <summary>
        /// Used to specify the locations that the view engine should search to 
        /// locate views.
        /// </summary>
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // {2} is area, {1} is controller,{0} is the action
            var partialViewLocationFormats = new[] {
                "~/Views/Modules/{0}.cshtml",
                "~/Views/Layouts/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml" };

            var viewLocationFormats = new[] {
                "~/Views/Modules/{0}.cshtml",
                "~/Views/Pages/{1}.cshtml",
                "~/Views/Modules/{1}.cshtml",
                "~/Views/Shared/{0}.cshtml" };

            return context.IsMainPage ? viewLocationFormats : partialViewLocationFormats;
        }

        public virtual void PopulateValues(ViewLocationExpanderContext context) =>
            context.Values["customviewlocation"] = nameof(ViewLocationExpander);
    }
}