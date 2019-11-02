using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    class BlobViewModelModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType.IsA<BlobViewModel>())
                bindingContext.Result = ModelBindingResult.Success(Bind(bindingContext));
            else
                throw new InvalidOperationException();
        }

        BlobViewModel Bind(ModelBindingContext bindingContext)
        {
            var result = new BlobViewModel
            {
                Action = GetValue(bindingContext, "Action")?.To<BlobViewModelAction>() ?? BlobViewModelAction.New,
                Filename = GetValue(bindingContext, "Filename"),
                ItemId = GetValue(bindingContext, "ItemId"),
                TempFileId = GetValue(bindingContext, "TempFileId"),
                Url = GetValue(bindingContext, "Url")
            };

            if (result.Action == BlobViewModelAction.New && result.TempFileId.IsEmpty())
                return null;

            result.IsEmpty = result.TempFileId.HasValue() ||
                (result.Filename.HasValue() && result.Filename != Blob.EMPTY_FILE);

            return result;
        }

        string GetValue(ModelBindingContext context, string propName) =>
            context.ValueProvider.GetValue($"{context.ModelName}_{propName}").FirstValue;
    }
}
