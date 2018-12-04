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
        public List<ExportedField> Fields = new List<ExportedField>();

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

        public override Type DomainType => domainType ?? (domainType = GetType().BaseType.GenericTypeArguments.Single());

        protected virtual string QueueKey => GetType().FullName;

        internal override void Start()
        {
            GlobalEntityEvents.InstanceSaved.Handle(async x =>
            {
                if (!x.Entity.GetType().IsA(DomainType)) return;
                await EventBus.Publish(QueueKey, ToMessage(x.Entity));
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
            DomainType.GetProperties()
               .Where(x => x.CanRead && x.CanWrite)
               .Where(x => x.DeclaringType.Assembly == DomainType.Assembly)
               .Do(v => Fields.Add(new ExportedField(v)));
        }
    }

    public class ExportedField
    {
        string title;
        public PropertyInfo Property { get; }

        public ExportedField(PropertyInfo property)
        {
            Property = property;
            title = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName;
            if (title.IsEmpty())
                title = property.Name.ToLiteralFromPascalCase();
        }

        public string GetTitle() => title;

        public ExportedField Title(string exportTitle)
        {
            title = exportTitle;
            return this;
        }
    }
}