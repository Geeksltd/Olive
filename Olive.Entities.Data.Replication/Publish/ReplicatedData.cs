using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ReplicatedData
    {
        internal string QueueUrl { get; set; }

        public List<ExportedField> Fields = new List<ExportedField>();

        public abstract Type DomainType { get; }

        public ReplicateDataMessage ToMessage(IEntity entity)
        {
            var properties = new Dictionary<string, object>();

            properties["ID"] = entity.GetId();

            foreach (var f in Fields.Except(x => x.IsInverseAssociation))
                properties[f.Property.Name] = f.GetValue(entity);

            var serialized = JsonConvert.SerializeObject(properties);

            return new ReplicateDataMessage
            {
                TypeFullName = GetType().FullName,
                Entity = serialized,
                CreationUtc = DateTime.UtcNow
            };
        }

        internal abstract void Start();

        internal abstract Task UploadAll();

        protected internal abstract void Define();
    }
}