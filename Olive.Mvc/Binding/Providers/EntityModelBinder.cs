using Microsoft.AspNetCore.Mvc.ModelBinding;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CustomEntityBinder = System.Func<string, System.Threading.Tasks.Task<Olive.Entities.IEntity>>;

namespace Olive.Mvc
{
    public partial class EntityModelBinder : IModelBinder
    {
        Type EntityType;
        IDatabaseProviderConfig ProviderConfig;

        public EntityModelBinder(Type type)
        {
            EntityType = type;
            CustomBinder = FindCustomParser(EntityType);
            ProviderConfig = Context.Current.GetService<IDatabaseProviderConfig>();
        }

        bool IsTransient => TransientEntityAttribute.IsTransient(EntityType);

        static IDatabase Database => Context.Current.Database();

        #region GuidEntityReadableTextParsers

        static Dictionary<Type, Func<string, Task<GuidEntity>>> GuidEntityReadableTextParsers
            = new Dictionary<Type, Func<string, Task<GuidEntity>>>();

        /// <summary>
        /// If you want to use the string format of an guid entity in URL, then you can get MVC to bind the entity directly from a textual route value. This registers your binding in addition to the normal binding from GUID.
        /// </summary>
        public static void RegisterReadableTextParser<TEntity>(Func<string, Task<GuidEntity>> binder) where TEntity : GuidEntity
        {
            GuidEntityReadableTextParsers.Add(typeof(TEntity), binder);
        }

        static async Task<GuidEntity> ParseGuidEntityFromReadableText(Type entityType, string data)
        {
            var actualType = entityType;

            while (true)
            {
                var binder = GuidEntityReadableTextParsers.TryGet(actualType);

                if (binder != null) return await binder(data);

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



        #endregion

        object FromRequestValues(ModelBindingContext bindingContext)
        {
            var result = EntityType.CreateInstance();
            // Parse from values:
            foreach (var p in EntityType.GetProperties().Where(x => x.CanWrite))
            {
                var requestValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + p.Name).FirstValue;

                if (requestValue.HasValue())
                    p.SetValue(result, requestValue.To(p.PropertyType));
            }

            return result;
        }

        bool ShouldInitializeFromRequestValues()
        {
            return IsTransient && CustomBinder == null && ProviderConfig.TryGetProvider(EntityType) == null;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (IsReadOnly(bindingContext)) return;

            var data = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

            if (ShouldInitializeFromRequestValues())
            {
                bindingContext.Result = ModelBindingResult.Success(FromRequestValues(bindingContext));
                return;
            }
            else
            {
                // No value provided, skip binding:
                if (data.IsEmpty()) return;
            }

            try
            {
                bindingContext.Result = ModelBindingResult.Success(await Bind(data));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to bind the value of type '{EntityType.FullName}' from '{data}'.", ex);
            }
        }

        async Task<IEntity> Bind(string data)
        {
            if (data.IsAnyOf("{NULL}", "-", Guid.Empty.ToString())) return null;

            if (CustomBinder != null)
                return await CustomBinder(data);

            IEntity result;

            if (EntityType.IsA<GuidEntity>() && !data.Is<Guid>())
            {
                // We have some data which is not Guid.
                result = await ParseGuidEntityFromReadableText(EntityType, data);
            }
            else
            {
                result = await Database.GetOrDefault(data, EntityType);
            }

            // Sometimes (e.g. in master detail binding) the view model data is written to.
            // So it must be cloned.
            return result?.Clone();
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
