using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    class PrimitiveValueModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).Get(x => x.FirstValue).OrEmpty();

            bindingContext.Result = ModelBindingResult.Success(await ViewModelServices.Convert(value, bindingContext.ModelType));
        }
    }
}
