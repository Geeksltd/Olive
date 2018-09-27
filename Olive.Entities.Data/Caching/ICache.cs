using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data
{
    public interface ICache
    {
        void Add(IEntity entity);
        void Remove(IEntity entity);
        void Remove(Type type, bool invalidateCachedReferences = false);
        void ExpireLists(Type type);
        IEnumerable GetList(Type type, string key);
        void ClearAll();
        void AddList(Type type, string key, IEnumerable list);
        bool IsUpdatedSince(IEntity instance, DateTime since);
        void UpdateRowVersion(IEntity entity);
        TEntity Get<TEntity>(object id) where TEntity : IEntity;
        IEntity Get(Type type, string id);
        DateTime? GetQueryTimestamp();
    }
}
