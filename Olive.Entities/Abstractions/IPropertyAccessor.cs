using System.Data;

namespace Olive.Entities
{
    public interface IPropertyAccessor
    {
        void Set(IEntity entity, IDataReader reader, int index);

        void Set(IEntity entity, object value);

        object Get(IEntity entity);
    }
}
