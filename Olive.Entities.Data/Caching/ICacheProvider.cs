using System;
using System.Collections;

namespace Olive.Entities.Data
{
    public interface ICacheProvider
    {
        IEntity Get(Type entityType, string id);
        void Add(IEntity entity);
        void Remove(IEntity entity);
        void Remove(Type type, bool invalidateCachedReferences = false);
        void AddList(Type type, IEnumerable list);
        void RemoveList(Type type);
        IEnumerable GetList(Type type);
        void ClearAll();
        bool IsUpdatedSince(IEntity instance, DateTime since);
        void UpdateRowVersion(IEntity entity);
    }
}
