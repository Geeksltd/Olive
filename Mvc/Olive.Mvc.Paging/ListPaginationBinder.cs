using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class ListPaginationBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var prefix = bindingContext.ModelName.OrEmpty().Unless("p");
            if (prefix.EndsWith(".p")) prefix = prefix.Split('.').ExceptLast().ToString(".");

            var old = bindingContext.Model as ListPagination;

            var result = old;
            if (value.FirstValue.HasValue())
            {
                result = new ListPagination(old?.Container, value.FirstValue);
                if (old != null)
                {
                    result.UseAjaxPost = old.UseAjaxPost;
                    result.UseAjaxGet = old.UseAjaxGet;
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}