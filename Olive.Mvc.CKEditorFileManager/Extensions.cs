using Microsoft.Extensions.DependencyInjection;
using Olive.Mvc.CKEditorFileManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Mvc
{
    public static class Extensions
    {
        public static IMvcBuilder AddCKEditorFileManager(this IMvcBuilder @this)
        {
            return @this.AddApplicationPart(typeof(ICKEditorFile).Assembly)
                .AddControllersAsServices();
        }
    }
}
