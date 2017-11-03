// namespace Olive.Mvc
// {
//    using System;
//    using System.Collections;
//    using System.Collections.Concurrent;
//    using System.Collections.Generic;
//    using System.Collections.Specialized;
//    using System.ComponentModel;
//    using System.Linq;
//    using System.Reflection;

//    public class OliveModelBinder : DefaultModelBinder
//    {
//        static ConcurrentDictionary<Type, MethodInfo> CustomBindMethods = new ConcurrentDictionary<Type, MethodInfo>();
//        protected override PropertyDescriptorCollection GetModelProperties(ControllerContext cContext, ModelBindingContext bContext)
//        {
//            var result = base.GetModelProperties(cContext, bContext).Cast<PropertyDescriptor>().ToList();
//            foreach (PropertyDescriptor prop in GetTypeDescriptor(cContext, bContext).GetProperties())
//            {
//                var fromQuery = prop.Attributes.OfType<FromQueryAttribute>().ToList();
//                if (fromQuery.None()) continue;

//                var old = result.Single(x => x.Name == prop.Name);
//                result.Remove(old);
//                result.AddRange(ByFromQuery(bContext, prop, fromQuery));
//            }

//            var sortedProperties = result.OrderBy(x => x.PropertyType.Implements<IViewModel>()).ToArray();
//            return new PropertyDescriptorCollection(sortedProperties);
//        }

//        /// <summary> Sets the specified property by using the specified controller context, binding context, and property value.</summary>
//        protected override void SetProperty(ControllerContext cContext, ModelBindingContext bContext, PropertyDescriptor pDescriptor, object value)
//        {
//            if (pDescriptor.PropertyType == typeof(string))
//            {
//                var stringValue = (string)value;
//                if (stringValue.HasValue())
//                {
//                    if (pDescriptor.Attributes.OfType<KeepWhiteSpaceAttribute>().None())
//                        stringValue = stringValue.Trim();
//                }

//                value = stringValue;
//            }

//            base.SetProperty(cContext, bContext, pDescriptor, value);
//        }

//        protected override void BindProperty(ControllerContext cContext, ModelBindingContext bContext, PropertyDescriptor propertyDescriptor)
//        {
//            if (propertyDescriptor.PropertyType.IsA<IViewModel>())
//            {
//                BindViewModelProperty(cContext, bContext, propertyDescriptor);
//            }
//            else if (propertyDescriptor.PropertyType.IsA<Blob>())
//            {
//                BindDocumentProperty(cContext, bContext, propertyDescriptor);
//            }
//            else if (propertyDescriptor.Attributes.OfType<MasterDetailsAttribute>().Any())
//            {
//                BindMasterDetailsProperty(cContext, bContext, propertyDescriptor);
//            }
//            else
//            {
//                base.BindProperty(cContext, bContext, propertyDescriptor);
//            }
//        }

//        void BindMasterDetailsProperty(ControllerContext cContext, ModelBindingContext bContext, PropertyDescriptor propertyDescriptor)
//        {
//            if ((cContext.Controller as Controller).Request.IsGet()) return;

//            var prefix = propertyDescriptor.Attributes.OfType<MasterDetailsAttribute>().Single().Prefix + "-";
//            var listObject = Activator.CreateInstance(propertyDescriptor.PropertyType) as IList;
//            var formData = cContext.RequestContext.HttpContext.Request.Form;

//            var childItemIds = formData.AllKeys.ExceptNull().Where(k => k.StartsWith(prefix) && k.EndsWith(".Item"))
//                .Select(x => x.TrimStart(prefix).TrimEnd(".Item")).ToList();

//            foreach (var id in childItemIds)
//            {
//                var formControlsPrefix = prefix + id + ".";
//                // The request form keys that are related to this row
//                var dataKeys = formData.AllKeys.ExceptNull().Where(x => x.StartsWith(formControlsPrefix)).ToList();
//                var instanceType = propertyDescriptor.PropertyType.GetGenericArguments().Single();
//                var instance = Activator.CreateInstance(instanceType);
//                listObject.Add(instance);
//                // Set the instance properties
//                foreach (var key in dataKeys)
//                {
//                    var propertyName = key.TrimStart(formControlsPrefix);
//                    var property = instanceType.GetProperty(propertyName);
//                    SetPropertyValue(cContext, bContext, instance, key, property, formData);
//                }

//                // All properties are written to ViewModel. Now also write them on the model (Item property):
//                var item = instance.GetType().GetProperty("Item").GetValue(instance);
//                ViewModelServices.CopyData(instance, item);
//            }

//            propertyDescriptor.SetValue(bContext.Model, listObject);
//        }

//        static void SetPropertyValue(ControllerContext cContext, ModelBindingContext bContext, object model, string modelName, PropertyInfo modelProperty, NameValueCollection formData)
//        {
//            // find the correct binding context for this property type
//            var modelBinder = OliveBinderProvider.SelectBinder(modelProperty.PropertyType);
//            object typed = null;
//            if (modelBinder != null)
//            {
//                var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, modelProperty.PropertyType);
//                var propertyContext = new ModelBindingContext { ValueProvider = bContext.ValueProvider, ModelMetadata = metadata, ModelName = modelName };
//                typed = modelBinder.BindModel(cContext, propertyContext);
//            }
//            else
//            {
//                // if Binder is null we've got a simple value type, throw it to view model services
//                try
//                {
//                    object data;

//                    if (modelProperty.Defines<AllowHtmlAttribute>())
//                        data = cContext.HttpContext.Request.Unvalidated.Form[modelName];
//                    else
//                        data = formData[modelName];

//                    typed = ViewModelServices.Convert(data, modelProperty.PropertyType);
//                }
//                catch
//                {
//                    // No logging is needed
//                    bContext.ModelState.AddModelError("Invalid Data", new Exception($"The value '{formData[modelName]}' is not valid for {modelProperty.Name}."));
//                }
//            }

//            // if we're still null at this point it's going to be a new entity
//            if (typed == null && modelProperty.PropertyType.IsA<IEntity>() && modelName.EndsWith(".Item"))
//                typed = Activator.CreateInstance(modelProperty.PropertyType);
//            modelProperty.SetValue(model, typed);
//        }

//        void BindDocumentProperty(ControllerContext cContext, ModelBindingContext bContext, PropertyDescriptor propertyDescriptor)
//        {
//            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, bContext.ModelType, propertyDescriptor.Name);
//            var pContext = new ModelBindingContext { ValueProvider = bContext.ValueProvider, ModelMetadata = metadata, ModelName = propertyDescriptor.Name };
//            var value = new DocumentModelBinder().BindModel(cContext, pContext);
//            if (value != null)
//                propertyDescriptor.SetValue(bContext.Model, value);
//        }

//        void BindViewModelProperty(ControllerContext cContext, ModelBindingContext bContext, PropertyDescriptor propertyDescriptor)
//        {
//            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, bContext.ModelType, propertyDescriptor.Name);
//            var pContext = new ModelBindingContext
//            {
//                ValueProvider = bContext.ValueProvider,
//                ModelMetadata = metadata, //ModelName = propertyDescriptor.Name
//            };
//            propertyDescriptor.SetValue(bContext.Model, BindModel(cContext, pContext));
//        }

//        IEnumerable<FromQueryPropertyDescriptor> ByFromQuery(ModelBindingContext bindingContext, PropertyDescriptor property, List<FromQueryAttribute> attributes)
//        {
//            foreach (var attr in attributes)
//            {
//                var metadata = bindingContext.PropertyMetadata;
//                if (metadata.ContainsKey(property.Name) && !metadata.ContainsKey(attr.Alias))
//                    metadata.Add(attr.Alias, metadata[property.Name]);
//                yield return new FromQueryPropertyDescriptor(attr.Alias, property);
//            }
//        }

//        protected override void OnModelUpdated(ControllerContext cContext, ModelBindingContext bContext)
//        {
//            base.OnModelUpdated(cContext, bContext);
//            if (bContext.Model is IViewModel)
//            {
//                OnPreBoundAttribute.Enqueue(cContext, bContext.Model);
//                OnBoundAttribute.Enqueue(cContext, bContext.Model);
//            }

//            // Imagine a root ViewModel object which has properties (i.e. nested objects) which are
//            // also ViewModel types themselves.
//            // The invokation lines below should in fact be called during OnModelUpdated of the root object only
//            // This is to ensure that all interim PreBinds happen, before all OnBound ones.
//            // The following lines are called of course for all child objects (before calling it on the root object)
//            // But they will do nothing (return early) when called for non-Root objects.
//            OnPreBoundAttribute.InvokeAllForRoot(bContext, cContext.HttpContext);
//            OnBoundAttribute.InvokeAllForRoot(bContext, cContext.HttpContext);
//        }

//        protected override bool OnModelUpdating(ControllerContext cContext, ModelBindingContext bContext)
//        {
//            OnPreBoundAttribute.SetRoot(bContext, cContext.HttpContext);
//            OnBoundAttribute.SetRoot(bContext, cContext.HttpContext);
//            var result = base.OnModelUpdating(cContext, bContext);
//            if (bContext.Model is IViewModel)
//            {
//                OnPreBindingAttribute.Execute(cContext, bContext.Model);
//            }

//            return result;
//        }
//    }
// }