namespace Olive.Entities.Data
{
    using Olive;
    using Olive.Entities;
    using System.Threading.Tasks;

    public class EncryptedEntityLoadedInterceptor : EncryptedEntityInterceptor, IEntityLoadedInterceptor
    {
        public Task Process(IEntity entity) => Process(entity, (c, p) => Security.Encryption.Decrypt(c, p));
    }
}