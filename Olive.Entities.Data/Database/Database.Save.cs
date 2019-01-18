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

        /// <summary>
        /// Inserts or updates an object in the database.
        /// </summary>
        public async Task<T> Save<T>(T entity) where T : IEntity
        {
            await Save(entity as IEntity, SaveBehaviour.Default);
            return entity;
        }

        /// <summary>
        /// Inserts or updates an object in the database.
        /// </summary>        
        public async Task Save(IEntity entity, SaveBehaviour behaviour)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Func<Task> save = async () => await DoSave(entity, behaviour);

            Func<Task> doSave = async () =>
            {
                if (entity.IsNew) await save();
                else using (await GetSyncLock(entity.GetType().FullName + entity.GetId()).Lock()) await save();
            };

            if (Configuration.Transaction.EnforceForSave) await EnlistOrCreateTransaction(doSave);
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
                if (mode == SaveMode.Insert) await Audit.Audit.LogInsert(entity);
                else await Audit.Audit.LogUpdate(entity);

            await dataProvider.Save(entity);
            Cache.UpdateRowVersion(entity);

            if (mode == SaveMode.Update && asEntity?._ClonedFrom != null && AnyOpenTransaction())
            {
                asEntity._ClonedFrom.IsStale = true;
                asEntity.IsStale = false;
            }

            if (mode == SaveMode.Insert)
                Entity.Services.SetSaved(entity, setOriginalId: false);

            Cache.Remove(entity);

            if (Transaction.Current != null)
                Transaction.Current.TransactionCompleted += (s, e) => { Cache.Remove(entity); };

            DbTransactionScope.Root?.OnTransactionCompleted(() => Cache.Remove(entity));

            await OnUpdated(entity);

            if (!IsSet(behaviour, SaveBehaviour.BypassSaved))
                await Entity.Services.RaiseOnSaved(entity, new SaveEventArgs(mode));

            // OnSaved event handler might have read the object again and put it in the cache, which would
            // create invalid CachedReference objects.
            Cache.Remove(entity);
        }

        /// <summary>
        /// Saves the specified records in the data repository.
        /// The operation will run in a Transaction.
        /// </summary>
        public Task<IEnumerable<T>> Save<T>(List<T> records) where T : IEntity => Save(records as IEnumerable<T>);

        /* ===================== Update ========================*/

        /// <summary>
        /// Runs an update command on a list of given objects and persists the updated objects in database.
        /// It returns the updated instances.
        /// </summary>
        /// <param name="items">The objects to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        public async Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action) where T : IEntity =>
            await Update<T>(items, action, SaveBehaviour.Default);

        /// <summary>
        /// Runs an update command on a list of given objects and persists the updated objects in database.
        /// It returns the updated instances.
        /// </summary>
        /// <param name="items">The objects to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        public async Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action, SaveBehaviour behaviour) where T : IEntity
        {
            var result = new List<T>();

            await EnlistOrCreateTransaction(async () =>
            {
                foreach (var item in items)
                    result.Add(await Update(item, action, behaviour));
            });

            return result;
        }

        /// <summary>
        /// Runs an update command on a given object's clone and persists the updated object in database. It returns the updated instance.
        /// </summary>
        /// <param name="item">The object to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        public Task<T> Update<T>(T item, Action<T> action) where T : IEntity => Update<T>(item, action, SaveBehaviour.Default);

        /// <summary>
        /// Runs an update command on a given object's clone and persists the updated object in database. It returns the updated instance.
        /// </summary>
        /// <param name="item">The object to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        public async Task<T> Update<T>(T item, Action<T> action, SaveBehaviour behaviour) where T : IEntity
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (item.IsNew)
                throw new InvalidOperationException("New instances cannot be updated using the Update method.");

            if (!(item is Entity))
                throw new ArgumentException($"Database.Update() method accepts a type inheriting from {typeof(Entity).FullName}. So {typeof(T).FullName} is not supported.");

            if ((item as Entity)._ClonedFrom?.IsStale == true && AnyOpenTransaction())
                // No need for an error. We can just get the fresh version here.
                item = await Reload(item);

            if (Entity.Services.IsImmutable(item as Entity))
            {
                var clone = (T)((IEntity)item).Clone();

                action(clone);

                await Save(clone as Entity, behaviour);

                if (!AnyOpenTransaction()) action(item);

                return clone;
            }
            else
            {
                action(item);
                await Save(item, behaviour);

                return item;
            }
        }

        /// <summary>
        /// Inserts the specified objects in bulk. None of the object events will be triggered.
        /// </summary>
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

        /// <summary>
        /// Updates the specified objects in bulk. None of the object events will be triggered.
        /// </summary>
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