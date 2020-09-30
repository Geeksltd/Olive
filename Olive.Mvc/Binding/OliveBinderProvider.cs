using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;
using Olive.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Olive.Mvc
{
    public class OliveBinderProvider : IModelBinderProvider
    {
        static readonly Type[] PrimitiveTypes = {typeof(DateTime), typeof(DateTime?), typeof(TimeSpan),
                typeof(TimeSpan?), typeof(bool), typeof(bool?)};

        static readonly ConcurrentDictionary<ModelMetadata, ModelMetadata[]> PropertyBinderCache = new ConcurrentDictionary<ModelMetadata, ModelMetadata[]>();

        IModelBinder IModelBinderProvider.GetBinder(ModelBinderProviderContext context)
        {
            var modelType = context.Metadata.ModelType;

            if (modelType.Defines<ModelBinderAttribute>()) return null;
            if (modelType.IsA<IViewModel>())
            {
                var propertyBinders = GetProperties(context.Metadata)
                    .ToDictionary(property => property, context.CreateBinder);

                return new OliveModelBinder(propertyBinders, Context.Current.GetService<ILoggerFactory>());
            }

            if (modelType.IsA<ListSortExpression>()) return new ListSortExpressionBinder();

            if (modelType.IsA<ColumnSelection>()) return new ColumnSelectionBinder();
            if (modelType.IsA<IEntity>()) return new EntityModelBinder(modelType);
            if (modelType.IsA<BlobViewModel>()) return new BlobViewModelModelBinder();

            if (PrimitiveTypes.Contains(modelType)) return new PrimitiveValueModelBinder();

            if (TryGetListModelBinder(modelType, out var binder))
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
            if (modelType.IsA<List<long>>()) modelBinder = new ListModelBinder<long>();
            if (modelType.IsA<List<bool>>()) modelBinder = new ListModelBinder<bool>();
            if (modelType.IsA<List<double>>()) modelBinder = new ListModelBinder<double>();
            if (modelType.IsA<List<decimal>>()) modelBinder = new ListModelBinder<decimal>();
            if (modelType.IsA<List<DateTime>>()) modelBinder = new ListModelBinder<DateTime>();
            if (modelType.IsA<List<TimeSpan>>()) modelBinder = new ListModelBinder<TimeSpan>();

            return modelBinder != null;
        }

        static IEnumerable<ModelMetadata> GetProperties(ModelMetadata metadata)
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
            if (!modelType.IsGenericType) return false;
            if (!modelType.GetGenericArguments().First().IsEnum) return false;

            return modelType.GetGenericTypeDefinition().FullName.StartsWith("System.Collections.Generic.List");
        }

        static bool IsListOfEntity(Type modelType)
        {
            if (!modelType.IsGenericType) return false;
            if (!modelType.GetGenericArguments().First().IsA<IEntity>()) return false;

            return modelType.GetGenericTypeDefinition().FullName.StartsWith("System.Collections.Generic.List");
        }
    }
}