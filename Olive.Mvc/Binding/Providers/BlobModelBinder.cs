using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olive.Entities;

namespace Olive.Mvc
{
    class BlobModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).Get(x => x.FirstValue).OrEmpty();

            if (bindingContext.ModelType.IsA<Blob>())
                bindingContext.Result = ModelBindingResult.Success(await BindDocument(value));
            else
                bindingContext.Result = ModelBindingResult.Success((await BindDocuments(value)).ToList());
        }

        internal async Task<Blob> BindDocument(string value)
        {
            var docs = (await BindDocuments(value)).ToList();

            return docs.FirstOrDefault() ?? new Blob(new byte[0], "«UNCHANGED»");
        }

        internal async Task<IEnumerable<Blob>> BindDocuments(string value)
        {
            if (value.IsEmpty() || value == "KEEP")
                return new Blob[0];

            else if (value == "REMOVE")
                return new[] { Blob.Empty() };

            else
                return await value.Split('|').Trim()
                    .Where(x => x.StartsWith("file:"))
                    .Select(async id => await new FileUploadService().Bind(id))
                    .AwaitAll();
        }
    }
}
