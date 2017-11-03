using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    public class ListPaginationBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null) return null;

            var prefix = bindingContext.ModelName.OrEmpty().Unless("p");
            if (prefix.EndsWith(".p")) prefix = prefix.Split('.').ExceptLast().ToString(".");

            var old = bindingContext.Model as ListPagination;

            bindingContext.Result = ModelBindingResult.Success(new ListPagination(old.Container, value.FirstValue)
            {
                Prefix = prefix,
                UseAjaxPost = old.UseAjaxPost,
                UseAjaxGet = old.UseAjaxGet
            });

            return Task.CompletedTask;
        }
    }
}