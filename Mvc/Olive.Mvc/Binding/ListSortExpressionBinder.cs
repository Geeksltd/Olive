using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class ListSortExpressionBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null) return Task.CompletedTask;

            var old = bindingContext.Model as ListSortExpression;

            bindingContext.Result = ModelBindingResult.Success(new ListSortExpression(old.Container, value.FirstValue)
            {
                UseAjaxPost = old.UseAjaxPost,
                Prefix = old.Prefix
            }
            );

            return Task.CompletedTask;
        }
    }
}