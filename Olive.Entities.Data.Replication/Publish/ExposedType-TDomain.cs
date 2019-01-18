using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ExposedType<TDomain> : ExposedType where TDomain : class, IEntity
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
            GlobalEntityEvents.InstanceSaved.Handle(x => OnInstanceSaved(x.Entity));
        }

        async Task OnInstanceSaved(IEntity item)
        {
            Logger.Debug("Instance saved. Initiating publish checks.");

            if (!(item is TDomain entity))
            {
                Logger.Debug("Publish aborted: " + item.GetType().Name + " is not of type " + DomainType.Name);
                return;
            }

            if (!Filter(entity) || !(await FilterAsync(entity)))
            {
                Logger.Debug("Skipped publishing the " + entity.GetType().Name + " record of " + entity.GetId() + ". It does not match the filter.");
                return;
            }

            Logger.Debug("Publishing the " + entity.GetType().Name + " record of " + entity.GetId());
            await Queue.Publish(await ToMessage(entity));
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
            foreach (var item in await Context.Current.Database().GetList<TDomain>())
                await Queue.Publish(await ToMessage(item)); // TODO: Should this be done in parallel batches?
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