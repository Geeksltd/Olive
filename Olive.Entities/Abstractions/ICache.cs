using System;
using System.Collections;

namespace Olive.Entities
{
    public interface ICache
    {
        void Add(IEntity entity);
        void Remove(IEntity entity);
        void Remove(Type type, bool invalidateCachedReferences = false);
        void RemoveList(Type type);
        IEnumerable GetList(Type type);
        void ClearAll();
        void AddList(Type type, IEnumerable list);
        bool IsUpdatedSince(IEntity instance, DateTime since);
        void UpdateRowVersion(IEntity entity);
        TEntity Get<TEntity>(object id) where TEntity : IEntity;
        IEntity Get(Type type, string id);
        DateTime? GetQueryTimestamp();
        bool IsCacheable(Type type);
    }
}
