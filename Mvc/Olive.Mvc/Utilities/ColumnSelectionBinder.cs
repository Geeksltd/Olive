using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    public class ColumnSelectionBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var result = bindingContext.Model as ColumnSelection;

            result?.SetSelection(value.FirstValue.OrEmpty().Split('|'));

            return Task.CompletedTask;
        }
    }
}