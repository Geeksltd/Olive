using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var f in Fields.Where(x => x.ShouldSerialize()))
                properties[f.GetName()] = f.GetSerializableValue(entity);

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