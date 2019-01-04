using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ExposedType
    {
        internal string QueueUrl { get; set; }

        public List<ExposedField> Fields = new List<ExposedField>();

        public abstract Type DomainType { get; }

        public async Task<ReplicateDataMessage> ToMessage(IEntity entity)
        {
            var properties = new Dictionary<string, object>();

            properties["ID"] = entity.GetId();

            foreach (var f in Fields.Where(x => x.ShouldSerialize()))
            {
                try
                {
                    Log.For(this).Debug("Finding the value of " + f.GetName() + " field");

                    var value = f.GetSerializableValue(entity);
                    if (value == null) properties[f.GetName()] = null;
                    else properties[f.GetName()] = await value;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to extract the serializable value of " + f.GetName() + " field.", ex);
                }
            }

            var serialized = JsonConvert.SerializeObject(properties);

            return new ReplicateDataMessage
            {
                TypeFullName = GetType().Namespace + "." + GetType().Name,
                Entity = serialized,
                CreationUtc = DateTime.UtcNow
            };
        }

        internal abstract void Start();

        internal abstract Task UploadAll();

        public abstract void Define();
    }
}