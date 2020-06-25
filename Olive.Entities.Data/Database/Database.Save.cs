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

            Func<Task> save = async () => await DoSave(entity, behaviour);

            Func<Task> doSave = async () =>
            {
                if (entity.IsNew) await save();
                else using (await GetSyncLock(entity.GetType().FullName + entity.GetId()).Lock()) await save();
            };

            if (ProviderConfig.Configuration.Transaction.EnforceForSave) await EnlistOrCreateTransaction(doSave);
            else await doSave();
        }

        async Task DoSave(IEntity entity, SaveBehaviour behaviour)
        {
            var mode = entity.IsNew ? SaveMode.Insert : SaveMode.Update;

            var asEntity = entity as Entity;
            if (mode == SaveMode.Update && (asEntity._ClonedFrom?.IsStale == true) && AnyOpenTransaction())
            {
                throw new InvalidOperationException("This " + entity.GetType().Name + " instance in memory is out-of-date. " +
                    "A clone of it is already updated in the transaction. It is not allowed to update the same instance multiple times in a transaction, because then the earlier updates would be overwriten by the older state of the instance in memory. \r\n\r\n" +
                    @"BAD: 
Database.Update(myObject, x=> x.P1 = ...); // Note: this could also be nested inside another method that's called here instead.
Database.Update(myObject, x=> x.P2 = ...);

GOOD: 
Database.Update(myObject, x=> x.P1 = ...);
myObject = Database.Reload(myObject);
Database.Update(myObject, x=> x.P2 = ...);");
            }

            if (Entity.Services.IsImmutable(entity))
                throw new ArgumentException("An immutable record must be cloned before any modifications can be applied on it. " +
                    $"Type={entity.GetType().FullName}, Id={entity.GetId()}.");

            var dataProvider = GetProvider(entity);

            if (!IsSet(behaviour, SaveBehaviour.BypassValidation))
            {
                await Entity.Services.RaiseOnValidating(entity as Entity, EventArgs.Empty);
                await entity.Validate();
            }
            else if (!dataProvider.SupportValidationBypassing())
            {
                throw new ArgumentException(dataProvider.GetType().Name + " does not support bypassing validation.");
            }

            #region Raise saving event

            if (!IsSet(behaviour, SaveBehaviour.BypassSaving))
            {
                var savingArgs = new System.ComponentModel.CancelEventArgs();
                await Entity.Services.RaiseOnSaving(entity, savingArgs);

                if (savingArgs.Cancel)
                {
                    Cache.Remove(entity);
                    return;
                }
            }

            #endregion

            if (!IsSet(behaviour, SaveBehaviour.BypassLogging))
                if (mode == SaveMode.Insert) await Audit.LogInsert(entity);
                else await Audit.LogUpdate(entity);

            await dataProvider.Save(entity);
            Cache.UpdateRowVersion(entity);

            if (mode == SaveMode.Update && asEntity?._ClonedFrom != null && AnyOpenTransaction())
            {
                asEntity._ClonedFrom.IsStale = true;
                asEntity.IsStale = false;
            }

            if (mode == SaveMode.Insert)
                Entity.Services.SetSaved(entity);

            Cache.Remove(entity);

            if (Transaction.Current != null)
                Transaction.Current.TransactionCompleted += (s, e) => { Cache.Remove(entity); };

            DbTransactionScope.Root?.OnTransactionCompleted(() => Cache.Remove(entity));

            await Updated.Raise(entity);

            if (!IsSet(behaviour, SaveBehaviour.BypassSaved))
                await Entity.Services.RaiseOnSaved(entity, new SaveEventArgs(mode));

            // OnSaved event handler might have read the object again and put it in the cache, which would
            // create invalid CachedReference objects.
            Cache.Remove(entity);
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
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!(action == null ^ asyncAction == null))
                throw new ArgumentNullException(nameof(action));

            if (item.IsNew)
                throw new InvalidOperationException("New instances cannot be updated using the Update method.");

            if (!(item is Entity))
                throw new ArgumentException($"Database.Update() method accepts a type inheriting from {typeof(Entity).FullName}. So {typeof(T).FullName} is not supported.");

            if ((item as Entity)._ClonedFrom?.IsStale == true && AnyOpenTransaction())
                // No need for an error. We can just get the fresh version here.
                item = await Reload(item);

            async Task doAction(T obj)
            {
                if (action == null)
                    await asyncAction(obj);
                else
                    action(obj);
            }

            if (Entity.Services.IsImmutable(item as Entity))
            {
                var clone = (T)((IEntity)item).Clone();

                await doAction(clone);

                await Save(clone as Entity, behaviour);

                if (!AnyOpenTransaction()) await doAction(item);

                return clone;
            }
            else
            {
                await doAction(item);
                await Save(item, behaviour);

                return item;
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