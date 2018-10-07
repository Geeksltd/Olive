namespace Olive.Entities.Data
{
    using Olive;
    using Olive.Entities;
    using System.Threading.Tasks;

    public class EncryptedEntitySavingInterceptor : EncryptedEntityInterceptor, IEntitySavingInterceptor
    {
        public Task Process(IEntity entity)
        {
            var properties = EncryptedProperties.GetOrDefault(entity.GetType());
            if (properties.None()) return Task.CompletedTask;

            foreach (var property in properties)
            {
                var clean = (string)property.GetValue(entity);
                if (clean.HasValue())
                    property.SetValue(entity, Security.Encryption.Encrypt(clean, EncryptionKey));
            }

            return Task.CompletedTask;
        }
    }
}