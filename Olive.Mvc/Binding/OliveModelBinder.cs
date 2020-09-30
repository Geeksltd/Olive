using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class OliveModelBinder : ComplexTypeModelBinder
    {
        readonly IDictionary<ModelMetadata, IModelBinder> PropertyBinders;

        public OliveModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory) : base(propertyBinders, loggerFactory) =>
            PropertyBinders = propertyBinders;

        protected override Task BindProperty(ModelBindingContext bindingContext)
        {
            Task result;

            var masterDetailAttribute = bindingContext.ModelMetadata.GetAttribute<MasterDetailsAttribute>();
            var fromRequestAttribute = bindingContext.ModelMetadata.GetAttribute<FromRequestAttribute>();

            if (masterDetailAttribute != null)
                result = BindMasterDetailsProperty(bindingContext, masterDetailAttribute);
            else
            {
                if (fromRequestAttribute != null)
                    bindingContext.ValueProvider = fromRequestAttribute.CreateValueProvider(bindingContext);

                result = base.BindProperty(bindingContext);
            }

            return result;
        }

        async Task BindMasterDetailsProperty(ModelBindingContext bindingContext, MasterDetailsAttribute attribute)
        {
            if (Context.Current.Request().IsGet()) return;

            var prefix = attribute.Prefix + "-";
            var listObject = Activator.CreateInstance(bindingContext.ModelType) as IList;

            var childItemIds = bindingContext.HttpContext.Request.Form.Select(x => x.Key).Trim()
                  .Where(k => k.StartsWith(prefix) && k.EndsWith(".Item"))
                  .Select(x => x.TrimStart(prefix).TrimEnd(".Item")).ToList();

            foreach (var id in childItemIds)
            {
                var formControlsPrefix = prefix + id + ".";

                var viewModel = Activator.CreateInstance(bindingContext.ModelMetadata.ElementType);
                listObject.Add(viewModel);

                foreach (var property in bindingContext.ModelMetadata.ElementMetadata.Properties)
                {
                    var key = formControlsPrefix + property.PropertyName;
                    await SetPropertyValue(bindingContext, viewModel, key, property);
                }

                // All properties are written to ViewModel. Now also write them on the model (Item property):
                var item = viewModel.GetType().GetProperty("Item").GetValue(viewModel);
                await ViewModelServices.CopyData(viewModel, item);
            }

            bindingContext.Result = ModelBindingResult.Success(listObject);
        }

        async Task SetPropertyValue(ModelBindingContext bindingContext, object model, string modelName, ModelMetadata property)
        {
            var fieldName = property.BinderModelName ?? property.PropertyName;

            ModelBindingResult result;
            using (bindingContext.EnterNestedScope(
                modelMetadata: property,
                fieldName: fieldName,
                modelName: modelName,
                model: model))
            {
                await base.BindProperty(bindingContext);

                if (bindingContext.ModelState.Keys.Contains(modelName) &&
                    bindingContext.ModelState[modelName].ValidationState == ModelValidationState.Unvalidated)
                    bindingContext.ModelState[modelName].ValidationState = ModelValidationState.Skipped;

                result = bindingContext.Result;
            }

            if (result.IsModelSet)
            {
                if (property.PropertyName == "Item" && result.Model == null)
                    result = ModelBindingResult.Success(Activator.CreateInstance(property.ModelType));

                property.PropertySetter(model, result.Model);
                // SetProperty(bindingContext, modelName, property, result);
            }
            else if (property.IsBindingRequired)
            {
                var message = property.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(fieldName);
                bindingContext.ModelState.TryAddModelError(modelName, message);
            }
        }
    }
}