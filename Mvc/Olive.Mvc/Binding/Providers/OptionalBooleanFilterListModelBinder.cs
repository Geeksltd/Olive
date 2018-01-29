using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    class OptionalBooleanFilterListModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.Result = ModelBindingResult.Success(value.FirstValue.OrEmpty().Split('|').Select(OptionalBooleanFilter.Parse).ExceptNull().ToList());

            return Task.CompletedTask;
        }
    }
}
