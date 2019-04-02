using Microsoft.Extensions.DependencyInjection;
using Olive.Mvc.CKEditorFileManager;

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
