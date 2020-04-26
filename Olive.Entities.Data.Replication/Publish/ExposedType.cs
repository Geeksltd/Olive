using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class HardDeletableExposedType : ExposedType
    {
        public override bool IsSoftDeleteEnabled => false;
    }

    public abstract class ExposedType
    {
        internal string QueueUrl { get; set; }

        public List<ExposedField> Fields = new List<ExposedField>();

        public abstract Type DomainType { get; }

        /// <summary>
        /// By default it's true. If you want the data to be hard-deleted on the target database, override this and return false.
        /// </summary>
        public virtual bool IsSoftDeleteEnabled => true;

        public async Task<ReplicateDataMessage> ToMessage(IEntity entity)
        {
            var properties = new Dictionary<string, object>();

            properties["ID"] = entity.GetId();

            foreach (var f in Fields.Where(x => x.ShouldSerialize()))
            {
                var fieldName = f.GetName();

                try
                {
                    Log.For(this).Debug($"Finding the value of {fieldName} field");

                    var value = f.GetSerializableValue(entity);
                    if (value == null) properties[f.GetName()] = null;
                    else
                    {
                        var valueData = await value;

                        if (valueData is IEntity e) valueData = e.GetId();

                        properties[f.GetName()] = Stringify(valueData);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to extract the serializable value of " + f.GetName() + " field.", ex);
                }
            }

            return ToReplicateDataMessage(properties);
        }

        string Stringify(object value)
        {
            if (value is DateTime dateTime)
                return dateTime.Ticks.ToString();

            return value.ToStringOrEmpty();
        }

        public ReplicateDataMessage ToDeleteMessage(IEntity entity)
        {
            var properties = new Dictionary<string, object>();

            properties["ID"] = entity.GetId();

            var message = ToReplicateDataMessage(properties);
            message.ToDelete = true;
            return message;
        }
        string GetTypeFullName() => GetType().Namespace + "." + GetType().Name;
        ReplicateDataMessage ToReplicateDataMessage(Dictionary<string, object> properties)
        {
            var serialized = JsonConvert.SerializeObject(properties);

            return new ReplicateDataMessage
            {
                TypeFullName = GetTypeFullName(),
                Entity = serialized,
                CreationUtc = DateTime.UtcNow
            };
        }

        protected ReplicateDataMessage ToClearMessage() => new ReplicateDataMessage
        {
            TypeFullName = GetTypeFullName(),
            CreationUtc = DateTime.UtcNow,
            IsClearSignal = true
        };

        internal abstract void Start();

        internal async Task UploadAll()
        {
            await SingalClear();

            await DoUploadAll();
        }

        protected abstract Task DoUploadAll();

        protected abstract Task SingalClear();

        public abstract void Define();
    }
}