using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Olive.Web;

namespace Olive.Mvc
{
    public class OliveModelBinder : ComplexTypeModelBinder
    {
        readonly IDictionary<ModelMetadata, IModelBinder> PropertyBinders;

        public OliveModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders) : base(propertyBinders) => 
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
            if (Context.Request.IsGet()) return;

            var prefix = attribute.Prefix + "-";
            var listObject = Activator.CreateInstance(bindingContext.ModelType) as IList;
            // var formData = cContext.RequestContext.HttpContext.Request.Form;

            var childItemIds = bindingContext.ValueProvider.GetValue(prefix + ".Item").FirstValue?.Split("|")?.ToArray() ?? new string[0];

            foreach (var id in childItemIds)
            {
                var formControlsPrefix = prefix + id + ".";

                var instanceType = bindingContext.ModelMetadata.ElementType;
                var instance = Activator.CreateInstance(instanceType);
                listObject.Add(instance);

                // Set the instance properties
                foreach (var property in bindingContext.ModelMetadata.ElementMetadata.Properties)
                {
                    var key = formControlsPrefix + property.PropertyName;

                    await SetPropertyValue(bindingContext, instance, key, property);
                }

                // All properties are written to ViewModel. Now also write them on the model (Item property):
                var item = instance.GetType().GetProperty("Item").GetValue(instance);
                await ViewModelServices.CopyData(instance, item);
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