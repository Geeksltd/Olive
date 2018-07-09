using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public interface IEntityLoadedInterceptor
    {
        Task Process(IEntity entity);
    }

    public interface IEntitySavingInterceptor
    {
        Task Process(IEntity entity);
    }
}