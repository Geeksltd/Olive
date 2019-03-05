namespace Olive.Entities.Data
{
    using Olive;
    using Olive.Entities;
    using System.Threading.Tasks;

    public class EncryptedEntitySavingInterceptor : EncryptedEntityInterceptor, IEntitySavingInterceptor
    {
        public Task Process(IEntity entity) => Process(entity, (c, p) => Security.Encryption.Encrypt(c, p));
    }
}