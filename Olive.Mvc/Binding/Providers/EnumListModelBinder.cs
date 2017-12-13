using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Olive.Mvc
{
    class EnumListModelBinder : IModelBinder
    {
        Type EnumType, ListType;
        public EnumListModelBinder(Type enumType, Type listType)
        {
            EnumType = enumType;
            ListType = listType;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var result = (IList)Activator.CreateInstance(ListType);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);

            else if (value.FirstValue != "{NULL}" || value.FirstValue.HasValue())
            {
                foreach (var ids in value.Values)
                {
                    foreach (var item in ids.Split('|').Trim())
                    {
                        var asInt = item.TryParseAs<int>();
                        if (asInt.HasValue)
                            result.Add(Enum.ToObject(EnumType, asInt));
                        else
                            result.Add(Enum.Parse(EnumType, item));
                    }
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}
