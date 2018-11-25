using Newtonsoft.Json;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Olive.Entities
{
    public abstract class ReplicatedData
    {
        protected List<ExportedField> Fields = new List<ExportedField>();

        public abstract Type DomainType { get; }

        public ReplicateDataMessage ToMessage(IEntity entity)
        {
            // TODO: Serialize based on the mappings.
            var serialized = JsonConvert.SerializeObject(entity);

            return new ReplicateDataMessage
            {
                Entity = serialized
            };
        }

        internal abstract void Start();
    }

    public abstract class ReplicatedData<TDomain> : ReplicatedData
        where TDomain : IEntity
    {
        Type domainType;
        protected abstract void Define();

        public override Type DomainType => domainType ?? (domainType = GetType().GetGenericArguments().Single());

        protected virtual string QueueKey => GetType().FullName;

        internal override void Start()
        {
            GlobalEntityEvents.InstanceSaved.Handle(async x =>
            {
                if (!x.Entity.GetType().IsA(DomainType)) return;
                await EventBus.Publish(QueueKey, ToMessage(x.Entity));
            });
        }

        public void Export<T>(Expression<Func<TDomain, T>> field)
        {
        }

        public void ExportAll()
        {
        }
    }

    public class ExportedField
    {
        public PropertyInfo Property { get; set; }
    }
}
