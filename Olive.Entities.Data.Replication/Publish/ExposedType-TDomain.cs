using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract partial class HardDeletableExposedType<TDomain> : ExposedType<TDomain> where TDomain : class, IEntity
    {
        public override bool IsSoftDeleteEnabled => true;
    }

    public abstract partial class ExposedType<TDomain> : ExposedType where TDomain : class, IEntity
    {
        Type domainType;
        ILogger Logger;

        public override Type DomainType => domainType ?? (domainType = GetType().BaseType.GenericTypeArguments.Single());

        protected virtual string QueueUrlConfigKey => string.Empty;

        IEventBusQueue Queue
        {
            get
            {
                if (QueueUrlConfigKey.HasValue())
                    return EventBus.Queue(Config.GetOrThrow(QueueUrlConfigKey));
                else
                    return EventBus.Queue(QueueUrl);
            }
        }

        internal override void Start()
        {
            Logger = Log.For(this);
            DependenciesEnitityTypes = Dependencies?.Select(p => p.EntityType)
                                                    .Union(typeof(TDomain))
                                                    .Distinct().ToArray();
            GlobalEntityEvents.InstanceSaved.Handle(x => OnInstanceSaved(x.Entity));
            GlobalEntityEvents.InstanceDeleted.Handle(x => OnInstanceDeleted(x));
        }

        async Task OnInstanceSaved(IEntity item)
        {
            Logger.Debug("Instance saved. Initiating publish checks.");

            if (!DependenciesEnitityTypes.Contains(item.GetType()))
            {
                Logger.Debug("Publish aborted: " + item.GetType().Name + " is not of type " + DomainType.Name);
                return;
            }

            if (item is TDomain entity)
            {
                await Publish(entity);
            }
            else if (item != null)
            {
                await
                    (await FindRelationsOf(item))
                    .ExceptNull()
                    .Do(async p => await Publish(p));
            }
        }

        async Task OnInstanceDeleted(GlobalDeleteEventArgs eventArg)
        {
            Logger.Debug("Instance saved. Initiating publish checks.");

            if (!DependenciesEnitityTypes.Contains(eventArg.EntityType))
            {
                Logger.Debug("Publish aborted: " + eventArg.EntityType.Name + " is not of type " + DomainType.Name);
                return;
            }

            if (eventArg.EntityType == typeof(TDomain))
            {
                if (IsSoftDeleteEnabled)
                    await Publish(eventArg.Entity as TDomain, toDelete: true);
            }
            else
            {
                await
                    (await FindRelationsOf(eventArg.Entity))
                    .ExceptNull()
                    .Do(async p => await Publish(p));
            }
        }

        async Task<IEnumerable<TDomain>> FindRelationsOf(IEntity item)
        {
            var selectedTriggers = Dependencies.Where(t => t.EntityType == item.GetType());

            return await selectedTriggers.SelectManyAsync(async t =>
            {
                try
                {
                    return (await t.Function.Invoke(item)) ?? new List<TDomain>();
                }
                catch
                {
                    return new List<TDomain>();
                }
            })?.Distinct()?.ToList();
        }

        async Task Publish(TDomain entity, bool toDelete = false)
        {
            if (!Filter(entity) || !(await FilterAsync(entity)))
            {
                Logger.Debug("Skipped publishing the " + entity.GetType().Name + " record of " + entity.GetId() + ". It does not match the filter.");
                return;
            }

            Logger.Debug("Publishing the " + entity.GetType().Name + " record of " + entity.GetId());

            if (!toDelete)
                await Queue.Publish(await ToMessage(entity));
            else
                await Queue.Publish(ToDeleteMessage(entity));
        }

        public ExposedPropertyInfo Expose<T>(Expression<Func<TDomain, T>> field)
        {
            var result = new ExposedPropertyInfo(field.GetProperty());
            Fields.Add(result);
            return result;
        }

        public CustomExposedField Expose<TProperty>(string title, Func<TDomain, TProperty> valueProvider)
        {
            return Expose(new CustomExposedField(title, typeof(TProperty), x => Task.FromResult<object>(valueProvider((TDomain)x))));
        }

        public CustomExposedField Expose<TProperty>(string title, Func<TDomain, Task<TProperty>> valueProvider)
        {
            return Expose(new CustomExposedField(title, typeof(TProperty),
             async x =>
             {
                 var value = valueProvider((TDomain)x);
                 if (value == null) return null;
                 return await value;
             }));
        }

        public CustomExposedField Expose(CustomExposedField field)
        {
            Fields.Add(field);
            return field;
        }

        public void ExposeEverything()
        {
            var properties = DomainType.GetProperties()
               .Where(x => x.CanRead && x.CanWrite)
               .Where(x => x.DeclaringType.Assembly == DomainType.Assembly)
               .ToArray();

            foreach (var p in properties)
            {
                if (p.PropertyType == typeof(Guid?) && p.Name.EndsWith("Id") && properties.Any(x => x.Name ==
               p.Name.TrimEnd(2))) continue;

                Fields.Add(new ExposedPropertyInfo(p));
            }
        }

        internal override async Task UploadAll()
        {
            var toUpload = await Context.Current.Database().GetList<TDomain>().ToArray();

            var log = Log.For(this);

            log.Warning("Uploading " + toUpload.Count() + " records of " + typeof(TDomain).FullName + " to the queue...");

            foreach (var item in toUpload)
            {
                IEventBusMessage message;
                try { message = await ToMessage(item); }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to create an event bus message for " + item.GetType().FullName + " with ID of " + item.GetId());
                    continue;
                }

                try { await Queue.Publish(message); }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to publish an event bus message for " + item.GetType().FullName + " with ID of " + item.GetId());
                    continue;
                }
            }

            log.Warning("Finished uploading " + toUpload.Count() + " records of " + typeof(TDomain).FullName + " to the queue.");
        }

        /// <summary>
        /// If it returns false for any given record, it will not be published in the replication queue.
        /// Warning: This will not unpublish or modify a record that was previously published, and which no longers meet the filter criteria.
        /// </summary>
        protected virtual Task<bool> FilterAsync(TDomain record) => Task.FromResult(true);

        /// <summary>
        /// If it returns false for any given record, it will not be published in the replication queue.
        /// Warning: This will not unpublish or modify a record that was previously published, and which no longers meet the filter criteria.
        /// </summary>
        protected virtual bool Filter(TDomain record) => true;
    }
}