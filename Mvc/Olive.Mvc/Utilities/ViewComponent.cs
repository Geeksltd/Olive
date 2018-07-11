namespace Olive.Mvc
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public abstract partial class ViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        protected async Task<TViewModel> Bind<TViewModel>(TViewModel instance = null)
            where TViewModel : class, IViewModel, new()
        {
            var result = instance ?? new TViewModel();

            var task = (HttpContext.Items["Controller"] as Controller)?.TryUpdateModelAsync(result);
            if (task != null) await task;

            return result;
        }
    }
}