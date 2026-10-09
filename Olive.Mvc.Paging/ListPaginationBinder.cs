using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class ListPaginationBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var old = bindingContext.Model as ListPagination;

            var result = old;

            // The view model's pagination has the list's settings, including its default page size.
            if (value.FirstValue.HasValue())
                result = new ListPagination(old, value.FirstValue);

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}