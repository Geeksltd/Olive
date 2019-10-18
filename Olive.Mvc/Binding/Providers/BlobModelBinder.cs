//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using Olive.Entities;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Olive.Mvc
//{
//    class BlobModelBinder : IModelBinder
//    {
//        public async Task BindModelAsync(ModelBindingContext bindingContext)
//        {
//            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

//            if (bindingContext.ModelType.IsA<Blob>())
//                bindingContext.Result = ModelBindingResult.Success(await BindDocument(value.OrEmpty()));
//            else
//                bindingContext.Result = ModelBindingResult.Success((await BindDocuments(value.OrEmpty())).ToList());
//        }

//        internal async Task<Blob> BindDocument(string value)
//        {
//            var docs = (await BindDocuments(value)).ToList();

//            return docs.FirstOrDefault() ?? new Blob(new byte[0], "«UNCHANGED»");
//        }

//        internal async Task<IEnumerable<Blob>> BindDocuments(string value)
//        {
//            if (value.IsEmpty() || value == "KEEP")
//                return new Blob[0];

//            else if (value == "REMOVE")
//                return new[] { Blob.Empty() };

//            else
//                return await value.Split('|').Trim()
//                    .Where(x => x.StartsWith("file:"))
//                    .SelectAsync(id => new FileUploadService().Bind(id));
//        }
//    }
//}
