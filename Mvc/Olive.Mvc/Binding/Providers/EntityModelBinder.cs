using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olive.Entities;

namespace Olive.Mvc
{
    public class EntityModelBinder : IModelBinder
    {
        static Dictionary<Type, Func<string, IEntity>> CustomParsers = new Dictionary<Type, Func<string, IEntity>>();

        static IDatabase Database => Context.Current.Database();

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
                    bindingContext.Result = ModelBindingResult.Success((await Database.GetOrDefault(data, bindingContext.ModelType))
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
}
