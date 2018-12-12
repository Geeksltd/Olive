using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ReplicatedData<TDomain> : ReplicatedData
        where TDomain : IEntity
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
            GlobalEntityEvents.InstanceSaved.Handle(async x =>
            {
                if (!x.Entity.GetType().IsA(DomainType)) return;
                await Queue.Publish(ToMessage(x.Entity));
            });
        }

        public ExportedField Export<T>(Expression<Func<TDomain, T>> field)
        {
            var result = new ExportedField(field.GetProperty());
            Fields.Add(result);
            return result;
        }

        public void ExportAll()
        {
            var properties = DomainType.GetProperties()
               .Where(x => x.CanRead && x.CanWrite)
               .Where(x => x.DeclaringType.Assembly == DomainType.Assembly)
               .ToArray();

            foreach (var p in properties)
            {
                if (p.PropertyType  == typeof(Guid?) && p.Name.EndsWith("Id") && properties.Any(x => x.Name ==
                p.Name.TrimEnd(2))) continue;

                Fields.Add(new ExportedField(p));
            }
        }

        internal override async Task UploadAll()
        {
            foreach (var item in await Context.Current.Database().GetList<TDomain>())
                await Queue.Publish(ToMessage(item)); // TODO: Should this be done in parallel batches?
        }
    }
}