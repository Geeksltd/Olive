using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Olive.Entities.Data
{
    partial class Database
    {
        static ConcurrentDictionary<string, AsyncLock> StringKeySyncLocks = new ConcurrentDictionary<string, AsyncLock>();

        public static AsyncLock GetSyncLock(string key) => StringKeySyncLocks.GetOrAdd(key, f => new AsyncLock());

        public async Task<T> Save<T>(T entity) where T : IEntity
        {
            await Save(entity as IEntity, SaveBehaviour.Default);
            return entity;
        }

        public async Task Save(IEntity entity, SaveBehaviour behaviour)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Task save() => new SaveOperation(this, entity, behaviour).Run();

            async Task doSave()
            {
                if (entity.IsNew) await save();
                else using (await GetSyncLock(entity.GetType().FullName + entity.GetId()).Lock()) await save();
            }

            if (ProviderConfig.Configuration.Transaction.EnforceForSave) await EnlistOrCreateTransaction(doSave);
            else await doSave();
        }

        public Task<IEnumerable<T>> Save<T>(List<T> records) where T : IEntity => Save(records as IEnumerable<T>);

        /* ===================== Update ========================*/

        public Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action) where T : IEntity =>
            Update(items, action, SaveBehaviour.Default);

        public Task<List<T>> Update<T>(IEnumerable<T> items, Func<T, Task> action) where T : IEntity =>
            Update(items, action, SaveBehaviour.Default);

        public Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action, SaveBehaviour behaviour) where T : IEntity =>
            Update(items, action, null, behaviour);

        public Task<List<T>> Update<T>(IEnumerable<T> items, Func<T, Task> action, SaveBehaviour behaviour) where T : IEntity =>
            Update(items, null, action, behaviour);

        async Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action, Func<T, Task> asyncAction, SaveBehaviour behaviour) where T : IEntity
        {
            var result = new List<T>();

            await EnlistOrCreateTransaction(async () =>
            {
                if (action != null)
                    foreach (var item in items)
                        result.Add(await Update(item, action, behaviour));
                else
                    foreach (var item in items)
                        result.Add(await Update(item, asyncAction, behaviour));
            });

            return result;
        }

        public Task<T> Update<T>(T item, Action<T> action) where T : IEntity => Update<T>(item, action, SaveBehaviour.Default);

        public Task<T> Update<T>(T item, Func<T, Task> action) where T : IEntity => Update<T>(item, action, SaveBehaviour.Default);

        public Task<T> Update<T>(T item, Action<T> action, SaveBehaviour behaviour) where T : IEntity =>
            Update(item, action, null, behaviour);

        public Task<T> Update<T>(T item, Func<T, Task> action, SaveBehaviour behaviour) where T : IEntity =>
            Update(item, null, action, behaviour);

        async Task<T> Update<T>(T item, Action<T> action, Func<T, Task> asyncAction, SaveBehaviour behaviour) where T : IEntity
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            if (!(action == null ^ asyncAction == null))
                throw new ArgumentNullException(nameof(action));

            if (item.IsNew)
                throw new InvalidOperationException("New instances cannot be updated using the Update method.");

            if (!(item is Entity entity))
                throw new ArgumentException($"Database.Update() method accepts a type inheriting from {typeof(Entity).FullName}. So {typeof(T).FullName} is not supported.");

            async Task doAction(object obj)
            {
                if (action == null) await asyncAction((T)obj);
                else action((T)obj);
            }

            if (Entity.Services.IsImmutable(entity))
            {
                var clone = entity.Clone();
                await doAction(clone);
                await Save(clone, behaviour);
                if (!AnyOpenTransaction()) await doAction(entity);
                return (T)clone;
            }
            else // entity is already cloned once
            {
                if (entity._ClonedFrom?.IsStale == true && AnyOpenTransaction())
                {
                    if (!ReferenceEquals(entity._ClonedFrom.UpdatedClone, entity))
                        // No need for an error. We can just get the fresh version here.
                        entity = await Reload(entity);
                }

                await doAction(entity);
                await Save(entity, behaviour);

                return (T)(object)entity;
            }
        }

        public async Task BulkInsert(Entity[] objects, int batchSize = 10, bool bypassValidation = false)
        {
            if (!bypassValidation)
                await objects.ValidateAll();

            var objectTypes = objects.GroupBy(o => o.GetType()).ToArray();

            try
            {
                foreach (var group in objectTypes)
                    await GetProvider(group.Key).BulkInsert(group.ToArray(), batchSize);

                foreach (var type in objectTypes)
                    Cache.Remove(type.Key);
            }
            catch
            {
                await Refresh();
                throw;
            }
        }

        public async Task BulkUpdate(Entity[] objects, int batchSize = 10, bool bypassValidation = false)
        {
            if (!bypassValidation)
                await objects.ValidateAll();

            var objectTypes = objects.GroupBy(o => o.GetType()).ToArray();

            try
            {
                foreach (var group in objectTypes)
                {
                    var records = group.ToArray();
                    await GetProvider(group.Key).BulkUpdate(records, batchSize);
                }

                foreach (var type in objectTypes)
                    Cache.Remove(type.Key);
            }
            catch
            {
                await Refresh();
                throw;
            }
        }
    }
}