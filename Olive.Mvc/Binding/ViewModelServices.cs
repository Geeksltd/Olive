namespace Olive
{
    using Entities;
    using Mvc;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class ViewModelServices
    {
        static IDatabase Database => Context.Current.Database();

        static BindingFlags PropertyFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.FlattenHierarchy | BindingFlags.SetProperty;

        public static async Task<T> To<T>(this IEntity model, string sourcePrefix = null, string targetPrefix = null) where T : IViewModel
        {
            var result = (T)Activator.CreateInstance(typeof(T));

            await model.CopyDataTo(result, sourcePrefix, targetPrefix);

            return result;
        }

        public static Task CopyDataTo(this IViewModel @this, IEntity to, string sourcePrefix = null, string targetPrefix = null)
        {
            return CopyData(@this, to, sourcePrefix, targetPrefix);
        }

        public static Task CopyDataTo(this IEntity @this, IViewModel to, string sourcePrefix = null, string targetPrefix = null)
        {
            return CopyData(@this, to, sourcePrefix, targetPrefix);
        }

        public static async Task CopyDataTo<TItem>(this Task<TItem> @this, IViewModel to, string sourcePrefix = null, string targetPrefix = null) where TItem : IEntity
        {
            await CopyData(await @this, to, sourcePrefix, targetPrefix);
        }

        static bool IsReadonly(PropertyInfo property) => property.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true;

        // [TODO]: Remove the following attribute
        [EscapeGCop("It takes time to fix this warning now. I will check it later.")]
        public static async Task CopyData(object from, object to, string sourcePrefix = null, string targetPrefix = null)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            foreach (var property in from.GetType().GetProperties(PropertyFlags))
            {
                if (sourcePrefix.HasValue() && !property.Name.StartsWith(sourcePrefix)) continue;
                // if (property.PropertyType.IsA<Task>()) continue;

                var inTarget = to.GetType().GetProperty(targetPrefix + property.Name.TrimStart(sourcePrefix.OrEmpty()), PropertyFlags);

                if (inTarget == null) continue;

                if (inTarget.GetSetMethod() == null) continue;

                if (inTarget.GetCustomAttributes<MasterDetailsAttribute>().Any()) continue;

                if (new[] { property, inTarget }.Any(p => p.GetCustomAttributes<CopyDataAttribute>().Any(x => x.CanCopy == false))) continue;

                if (from is IViewModel)
                {
                    if ((from as IViewModel).IsInvisible(property.Name) &&
                        // If it's readonly then it's perhaps set by a property setter or custom code
                        // as it can't be injected maliciously.
                        !IsReadonly(property))
                        continue; // Not visible                    
                }

                var sourceValue = property.GetValue(from);
                if (sourceValue is Task asTask)
                {
                    if (sourceValue.GetType().GenericTypeArguments.IsSingle())
                        sourceValue = sourceValue.GetType().GetProperty("Result").GetValue(sourceValue);
                    else continue;
                }

                if (from is IEntity && to is IViewModel)
                {
                    var hasClrDefaultValue = property.PropertyType.IsValueType &&
                        (from as IEntity).IsNew &&
                        inTarget.GetCustomAttributes<HasDefaultAttribute>().None();

                    var isValueDefault = sourceValue.ToStringOrEmpty() == property.PropertyType.GetDefaultValue().ToStringOrEmpty();

                    if (hasClrDefaultValue && isValueDefault) continue;
                }

                if (property.GetCustomAttributes<ReadOnlyAttribute>().Any(x => x.IsReadOnly))
                {
                    if (property.GetCustomAttributes<CustomBoundAttribute>().None()) continue;
                    if (sourceValue == null || string.Empty.Equals(sourceValue)) continue;
                    // Note: Property setters this way cannot set something to null.
                }

                try
                {
                    var value = await Convert(sourceValue, inTarget.PropertyType);

                    if (property.Defines<LocalizedDateAttribute>() && !inTarget.Defines<LocalizedDateAttribute>())
                        value = ((DateTime?)value).ToUniversal();

                    if (!property.Defines<LocalizedDateAttribute>() && inTarget.Defines<LocalizedDateAttribute>())
                        value = ((DateTime?)value).ToLocal();

                    if (inTarget.PropertyType == typeof(Blob))
                    {
                        if (from is IEntity && to is IViewModel)
                        {
                            (value as Blob).FolderName = (sourceValue as Blob).FolderName;
                        }
                        else if (from is IViewModel && (value == null || (value as Blob).IsUnchanged()))
                        {
                            // Null in view model means not changed.
                            continue;
                        }
                    }

                    if (value == null && inTarget.PropertyType.IsA<IViewModel>()) continue; // Don't set to null

                    inTarget.SetValue(to, value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not copy the {property.PropertyType} value of {{{sourceValue}}} from {from.GetType().Name}." +
                        $" {property.Name.TrimStart(sourcePrefix.OrEmpty())} to {inTarget.PropertyType} in {to.GetType().Name}", ex);
                }
            }
        }

        // [TODO]: Remove the following attribute
        [EscapeGCop("It takes time to fix this warning now. I will check it later.")]
        public static async Task<object> Convert(object source, Type target)
        {
            if (source == null) return null;

            if (source is Task asTask)
            {
                await asTask;
                source = source.GetType().GetPropertyOrThrow("Result").GetValue(source);
                if (source == null) return null;
            }

            if (target.IsA<Task>())
            {
                target = target.GetGenericArguments().Single();
                source = await ConvertNonTask(source, target);
                // Wrap the result as task:
                return typeof(Task).GetGenericMethod("FromResult", target).Invoke(null, new[] { source });
            }
            else return await ConvertNonTask(source, target);
        }

        [EscapeGCop("It takes time to fix this warning now. I will check it later.")]
        static async Task<object> ConvertNonTask(object source, Type target)
        {
            if (source is Blob && target.IsA<Blob>()) return await (source as Blob).CloneAsync(attach: true, @readonly: true);

            if (source.GetType().IsA(target)) return source;

            if (source is string && target == typeof(Guid?)) return (source as string).TryParseAs<Guid>();

            if (source is GuidEntity && target == typeof(Guid?)) return (source as GuidEntity).ID;

            if (target.IsA<IEntity>())
            {
                if (new[] { typeof(string), typeof(Guid?), typeof(Guid)
    }.Lacks(source.GetType()))
                {
                    throw new Exception($"Cannot convert {source.GetType().FullName} to {target.FullName}");
                }

                if (source.ToString().IsEmpty()) return null;

                else return (await Database.GetOrDefault(source.ToString(), target))?.Clone();
            }

            if (target.IsA<IViewModel>() && source is IEntity)
            {
                IViewModel result;
                if (target.GetConstructor(new[] { source.GetType() }) != null)
                    result = (IViewModel)Activator.CreateInstance(target, new object[] { source });
                else result = (IViewModel)Activator.CreateInstance(target);
                await (source as IEntity).CopyDataTo(result);
                return result;
            }

            if (source is string && target.Implements<IList>())
                return await ConvertCollection((source as string).Split('|'), target);

            if (source.GetType().Implements<IList>() && target == typeof(string))
                return (source as IEnumerable).ToString("|");

            if (source is IEnumerable)
            {
                if (target.IsA<IEnumerable<Blob>>())
                    throw new NotSupportedException();
                    //return await new BlobModelBinder().BindDocuments(source.ToStringOrEmpty());

                var result = await ConvertCollection(source as IEnumerable, target);
                if (result != null) return result;
            }

            if (source is IEntity)
            {
                if (target.IsA<IEntity>()) return source;
                else return Convert((source as IEntity).GetId(), target);
            }

            if (target.IsA<BlobViewModel>())
                return BlobViewModel.From(source as Blob);

            if (target.IsA<Blob>() && source is BlobViewModel viewModel)
                return await viewModel.ToBlob();

            try
            {
                return source.ToStringOrEmpty().To(target);
            }
            catch
            {
                throw;
            }
        }

        static async Task<IEnumerable> ConvertCollection(IEnumerable source, Type target)
        {
            IList result;
            Type objectType = null;

            if (target.IsA<IList>())
            {
                result = Activator.CreateInstance(target) as IList;
            }
            else if (target.IsA<IEnumerable<IEntity>>())
            {
                objectType = target.GetGenericArguments().Single();
                result = typeof(List<>).MakeGenericType(objectType).CreateInstance() as IList;
            }
            else return null;

            var targetHoldsIds = target.IsA<IList<Guid>>() || target.IsA<IList<string>>() || target.IsA<IList<int>>();

            foreach (var item in source)
            {
                if (item is IEntity)
                {
                    if (targetHoldsIds) result.Add((item as IEntity).GetId());
                    else result.Add(item);
                }
                else
                {
                    if (targetHoldsIds) result.Add(item);
                    else result.Add(await Database.Get(item, objectType));
                }
            }

            return result;
        }
    }
}