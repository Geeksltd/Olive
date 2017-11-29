using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    class ListModelBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);

            else if (value.FirstValue == "{NULL}" || value.FirstValue == Guid.Empty.ToString())
                bindingContext.Result = ModelBindingResult.Success(null);

            else
            {
                var result = new List<T>();

                // It is possible that data is sent as a single value but in pipeline seperated format.
                foreach (var idOrIds in value.Values)
                    result.AddRange(idOrIds.Split('|').Trim().Select(x => x.To<T>()));

                bindingContext.Result = ModelBindingResult.Success(result);
            }

            return Task.CompletedTask;
        }
    }
}
