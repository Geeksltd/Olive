using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    class OptionalBooleanFilterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.Result = ModelBindingResult.Success(OptionalBooleanFilter.Parse(value.FirstValue));

            return Task.CompletedTask;
        }
    }
}
