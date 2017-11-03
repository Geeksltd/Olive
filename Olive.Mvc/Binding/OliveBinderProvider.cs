using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Olive.Entities;

namespace Olive.Mvc
{
    public class OliveBinderProvider : IModelBinderProvider
    {
        static Type[] PrimitiveTypes = new[] {typeof(DateTime), typeof(DateTime?), typeof(TimeSpan),
                typeof(TimeSpan?), typeof(bool), typeof(bool?)};

        static ConcurrentDictionary<ModelMetadata, ModelMetadata[]> PropertyBinderCache = new ConcurrentDictionary<ModelMetadata, ModelMetadata[]>();

        IModelBinder IModelBinderProvider.GetBinder(ModelBinderProviderContext context) => SelectBinder(context);

        static internal IModelBinder SelectBinder(ModelBinderProviderContext context)
        {
            var modelType = context.Metadata.ModelType;

            if (modelType.IsA<IViewModel>())
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();

                foreach (var property in GetProperties(context.Metadata))
                    propertyBinders.Add(property, context.CreateBinder(property));

                return new OliveModelBinder(propertyBinders);
            }

            if (modelType.IsA<ListSortExpression>()) return new ListSortExpressionBinder();
            if (modelType.IsA<ListPagination>()) return new ListPaginationBinder();
            if (modelType.IsA<ColumnSelection>()) return new ColumnSelectionBinder();
            if (modelType.IsA<IEntity>()) return new EntityModelBinder();
            if (modelType.IsA<Blob>() || modelType.IsA<List<Blob>>()) return new DocumentModelBinder();

            if (PrimitiveTypes.Contains(modelType)) return new PrimitiveValueModelBinder();

            if (TryGetListModelBinder(modelType, out IModelBinder binder))
                return binder;

            if (modelType == typeof(OptionalBooleanFilter)) return new OptionalBooleanFilterModelBinder();
            if (modelType == typeof(List<OptionalBooleanFilter>)) return new OptionalBooleanFilterListModelBinder();

            if (IsListOfEnum(modelType))
                return new EnumListModelBinder(modelType.GetGenericArguments().First(), modelType);

            if (IsListOfEntity(modelType))
                return new EntityListModelBinder(modelType.GetGenericArguments().First(), modelType);

            return null;
        }

        [EscapeGCop("The nature of the try methods is to have out parameter.")]
        static bool TryGetListModelBinder(Type modelType, out IModelBinder modelBinder)
        {
            modelBinder = null;

            if (modelType.IsA<List<Guid>>()) modelBinder = new ListModelBinder<Guid>();
            if (modelType.IsA<List<string>>()) modelBinder = new ListModelBinder<string>();
            if (modelType.IsA<List<int>>()) modelBinder = new ListModelBinder<int>();
            if (modelType.IsA<List<bool>>()) modelBinder = new ListModelBinder<bool>();
            if (modelType.IsA<List<double>>()) modelBinder = new ListModelBinder<double>();
            if (modelType.IsA<List<decimal>>()) modelBinder = new ListModelBinder<decimal>();
            if (modelType.IsA<List<DateTime>>()) modelBinder = new ListModelBinder<DateTime>();
            if (modelType.IsA<List<TimeSpan>>()) modelBinder = new ListModelBinder<TimeSpan>();

            return modelBinder != null;
        }

        static ModelMetadata[] GetProperties(ModelMetadata metadata)
        {
            return PropertyBinderCache.GetOrAdd(metadata, x =>
            {
                var result = new List<ModelMetadata>(metadata.Properties.Count);

                foreach (var property in metadata.Properties)
                {
                    if (((DefaultModelMetadata)property).Attributes.Attributes.OfType<MasterDetailsAttribute>().Any())
                        result.AddRange(GetProperties(property.ElementMetadata));

                    result.Add(property);
                }

                return result.ToArray();
            });
        }

        static bool IsListOfEnum(Type modelType)
        {
            // if (modelType == null) return false;
            if (!modelType.IsGenericType) return false;
            if (!modelType.GetGenericArguments().First().IsEnum) return false; ;

            return modelType.GetGenericTypeDefinition().FullName.StartsWith("System.Collections.Generic.List");
        }

        static bool IsListOfEntity(Type modelType)
        {
            // if (modelType == null) return false;
            if (!modelType.IsGenericType) return false;
            if (!modelType.GetGenericArguments().First().IsA<IEntity>()) return false; ;

            return modelType.GetGenericTypeDefinition().FullName.StartsWith("System.Collections.Generic.List");
        }
    }

    public class EntityModelBinder : IModelBinder
    {
        static Dictionary<Type, Func<string, IEntity>> CustomParsers = new Dictionary<Type, Func<string, IEntity>>();

        #region GuidEntityReadableTextParsers

        static Dictionary<Type, Func<string, GuidEntity>> GuidEntityReadableTextParsers = new Dictionary<Type, Func<string, GuidEntity>>();

        /// <summary>
        /// If you want to use the string format of an guid entity in URL, then you can get MVC to bind the entity directly from a textual route value. This registers your binding in addition to the normal binding from GUID.
        /// </summary>
        public static void RegisterReadableTextParser<TEntity>(Func<string, GuidEntity> binder) where TEntity : GuidEntity
        {
            GuidEntityReadableTextParsers.Add(typeof(TEntity), binder);
        }

        static GuidEntity ParseGuidEntityFromReadableText(Type entityType, string data)
        {
            var actualType = entityType;

            while (true)
            {
                var binder = GuidEntityReadableTextParsers.TryGet(actualType);

                if (binder != null) return binder(data);

                if (actualType.BaseType == typeof(GuidEntity))
                {
                    // Not found:
                    throw new Exception($"Cannot parse the data '{data}' to {entityType.FullName} as no parser is registered for this type.\r\n" +
                        "Hint: Use EntityModelBinder.RegisterParser() to define your 'text to entity convertor' logic.");
                }
                else actualType = actualType.BaseType;
            }
        }

        #endregion

        #region Custom parsers

        /// <summary>
        /// Will register a custom binder for a type instead of the default which uses a Database.Get.
        /// </summary>
        public static void RegisterCustomParser<TEntity>(Func<string, IEntity> binder) where TEntity : IEntity
        {
            CustomParsers.Add(typeof(TEntity), binder);
        }

        static Func<string, IEntity> FindCustomParser(Type entityType)
        {
            Func<string, IEntity> result = null;

            foreach (var actualType in entityType.WithAllParents())
            {
                if (CustomParsers.TryGetValue(actualType, out result))
                    return result;
            }

            return null;
        }

        #endregion

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null) return;

            var data = value.FirstValue;

            // Special cases:
            if (data.IsEmpty() || data.IsAnyOf("{NULL}", "-", Guid.Empty.ToString())) return;

            if (IsReadOnly(bindingContext)) return;

            if (bindingContext.ModelType.IsA<GuidEntity>() && data.TryParseAs<Guid>() == null)
            {
                // We have some data which is not Guid.
                bindingContext.Result = ModelBindingResult.Success(ParseGuidEntityFromReadableText(bindingContext.ModelType, data));
            }

            var customBinder = FindCustomParser(bindingContext.ModelType);
            if (customBinder != null)
            {
                try
                {
                    bindingContext.Result = ModelBindingResult.Success(customBinder(data));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to bind the value of type '{bindingContext.ModelType.FullName}' from '{data}'.", ex);
                }
            }
            else
            {
                try
                {
                    bindingContext.Result = ModelBindingResult.Success((await Entity.Database.GetOrDefault(data, bindingContext.ModelType))
                        // Sometimes (e.g. in master detail binding) the view model data is written to the 'Item ', so it must be cloned.
                        ?.Clone());
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to bind the value of type '{bindingContext.ModelType.FullName}' from '{data}'.", ex);
                }
            }
        }

        bool IsReadOnly(ModelBindingContext context)
        {
            var metaData = context.ModelMetadata;
            if (metaData == null) return false;

            var type = metaData.ContainerType;
            if (type == null) return false;

            var propertyName = metaData.PropertyName;
            if (propertyName.IsEmpty()) return false;

            var property = type.GetProperty(propertyName);
            if (property == null) return false;

            return property.GetCustomAttributes<ReadOnlyAttribute>().Any(x => x.IsReadOnly);
        }
    }

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

    class OptionalBooleanFilterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.Result = ModelBindingResult.Success(OptionalBooleanFilter.Parse(value.FirstValue));

            return Task.CompletedTask;
        }
    }

    class OptionalBooleanFilterListModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (value == null)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.Result = ModelBindingResult.Success(value.FirstValue.OrEmpty().Split('|').Select(OptionalBooleanFilter.Parse).ExceptNull().ToList());

            return Task.CompletedTask;
        }
    }

    class DocumentModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).Get(x => x.FirstValue).OrEmpty();

            if (bindingContext.ModelType.IsA<Blob>())
                bindingContext.Result = ModelBindingResult.Success(BindDocument(value));
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

    class PrimitiveValueModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).Get(x => x.FirstValue).OrEmpty();

            bindingContext.Result = ModelBindingResult.Success(await ViewModelServices.Convert(value, bindingContext.ModelType));
        }
    }

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

    class EntityListModelBinder : IModelBinder
    {
        Type EntityType, ListType;
        public EntityListModelBinder(Type entityType, Type listType)
        {
            EntityType = entityType;
            ListType = listType;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
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
                        var item = Entity.Database.GetOrDefault(id, EntityType);

                        if (item != null) result.Add(item);
                    }
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}