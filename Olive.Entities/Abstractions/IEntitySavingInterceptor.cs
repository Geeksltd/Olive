using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public interface IEntitySavingInterceptor
    {
        Task Process(IEntity entity);
    }
}