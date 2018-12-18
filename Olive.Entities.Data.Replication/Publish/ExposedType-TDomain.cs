using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ExposedType<TDomain> : ExposedType where TDomain : IEntity
    {
        Type domainType;

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
            var log = Log.For(this);
            GlobalEntityEvents.InstanceSaved.Handle(async x =>
            {
                log.Debug("Instance saved. Initiating publish checks.");

                if (!x.Entity.GetType().IsA(DomainType))
                {
                    log.Debug("Publish aborted: " + x.Entity.GetType().Name + " is not of type " + DomainType.Name);
                    return;
                }
                else
                {
                    log.Debug("Publishing the " + x.Entity.GetType().Name + " record of " + x.Entity.GetId());
                    await Queue.Publish(await ToMessage(x.Entity));
                }
            });
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
                x => valueProvider((TDomain)x).AsTask<TProperty, object>()));
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
    }
}