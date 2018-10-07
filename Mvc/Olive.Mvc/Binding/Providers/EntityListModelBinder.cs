using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olive.Entities;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    class EntityListModelBinder : IModelBinder
    {
        static IDatabase Database => Context.Current.Database();

        Type EntityType, ListType;
        public EntityListModelBinder(Type entityType, Type listType)
        {
            EntityType = entityType;
            ListType = listType;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var result = (IList)Activator.CreateInstance(ListType);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);

            else
            {
                foreach (var idOrIds in value.Values)
                {
                    foreach (var id in idOrIds.OrEmpty().Split('|').Trim().Except("{NULL}", "-", Guid.Empty.ToString()))
                    {
                        var item = await Database.GetOrDefault(id, EntityType);

                        if (item != null) result.Add(item);
                    }
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);
        }
    }
}
