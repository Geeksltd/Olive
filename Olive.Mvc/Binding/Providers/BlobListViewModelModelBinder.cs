using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olive.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    class BlobListViewModelModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType.IsA<List<BlobViewModel>>())
                bindingContext.Result = ModelBindingResult.Success(Bind(bindingContext));
            else
                throw new InvalidOperationException();
        }

        IEnumerable<BlobViewModel> Bind(ModelBindingContext bindingContext)
        {
            var tempFileIds = GetValue(bindingContext, "TempFileId")?.Split('|').Trim().ToList();
            var filenames = GetValue(bindingContext, "Filename")?.Split(',').Trim().ToList();
            var itemIds = GetValue(bindingContext, "ItemId");
            var urls = GetValue(bindingContext, "Url");
            var result = new List<BlobViewModel>();

            if (tempFileIds?.Any() == true)
            {
                foreach (var tempFileId in tempFileIds.Select((value, index) => new { value, index }))
                {
                    var blobViewModel = new BlobViewModel
                    {
                        Action = GetValue(bindingContext, "Action")?.To<BlobViewModelAction>() ?? BlobViewModelAction.New,
                        Filename = filenames[tempFileId.index],
                        ItemId = itemIds,
                        TempFileId = tempFileId.value,
                        Url = urls,
                    };

                    blobViewModel.IsEmpty = blobViewModel.TempFileId.HasValue() ||
                        (blobViewModel.Filename.HasValue() && blobViewModel.Filename != Blob.EMPTY_FILE);

                    result.Add(blobViewModel);
                }
            }

            return result;
        }

        string GetValue(ModelBindingContext context, string propName) =>
           context.ValueProvider.GetValue($"{context.ModelName}_{propName}").FirstValue;
    }
}