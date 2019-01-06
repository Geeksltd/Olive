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
        void AddList(Type type, string key, IEnumerable list);
        void ExpireLists(Type type);
        IEnumerable GetList(Type type, string key);
        void ClearAll();
        bool IsUpdatedSince(IEntity instance, DateTime since);
        void UpdateRowVersion(IEntity entity);
    }
}
