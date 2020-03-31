using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Olive.Entities.Data
{
    partial class Database
    {
        /// <summary>
        /// Deletes the specified record from the data repository.
        /// </summary>
        public Task Delete(IEntity instance) => Delete(instance, DeleteBehaviour.Default);

        async Task DoDelete(Entity entity, DeleteBehaviour behaviour)
        {
            // Raise deleting event
            if (!IsSet(behaviour, DeleteBehaviour.BypassDeleting))
            {
                var deletingArgs = new System.ComponentModel.CancelEventArgs();
                await Entity.Services.RaiseOnDeleting(entity, deletingArgs);

                if (deletingArgs.Cancel)
                {
                    Cache.Remove(entity);
                    return;
                }
            }

            if (SoftDeleteAttribute.IsEnabled(entity.GetType()) && !SoftDeleteAttribute.Context.ShouldByPassSoftDelete())
            {
                SoftDeleteAttribute.MarkDeleted(entity);
                await GetProvider(entity).Save(entity);
            }
            else
            {
                await GetProvider(entity).Delete(entity);
            }
        }

        /// <summary>
        /// Deletes the specified record from the data repository.
        /// </summary>
        public async Task Delete(IEntity instance, DeleteBehaviour behaviour)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var entity = instance as Entity;

            if (entity == null)
                throw new ArgumentException($"The type of the specified object to delete does not inherit from {typeof(Entity).FullName} class.");

            await EnlistOrCreateTransaction(async () => await DoDelete(entity, behaviour));

            Cache.Remove(entity);
            if (Transaction.Current != null)
                Transaction.Current.TransactionCompleted += (s, e) => { Cache.Remove(entity); };

            DbTransactionScope.Root?.OnTransactionCompleted(() => Cache.Remove(entity));

            if (!IsSet(behaviour, DeleteBehaviour.BypassLogging))
                await Audit.LogDelete(entity);

            await Updated.Raise(entity);

            if (!IsSet(behaviour, DeleteBehaviour.BypassDeleted))
                await Entity.Services.RaiseOnDeleted(entity);
        }

        /// <summary>
        /// Deletes the specified instances from the data repository.        
        /// The operation will be done in a transaction.
        /// </summary>
        public async Task Delete<T>(IEnumerable<T> instances) where T : IEntity
        {
            if (instances == null)
                throw new ArgumentNullException(nameof(instances));

            if (instances.None()) return;

            await EnlistOrCreateTransaction(async () =>
            {
                foreach (var obj in instances.ToArray()) await Delete(obj);
            });
        }

        /// <summary>
        /// Deletes all objects of the specified type.
        /// </summary>
        public async Task DeleteAll<T>() where T : IEntity => await Delete(await GetList<T>());

        /// <summary>
        /// Deletes all objects of the specified type matching the given criteria.
        /// </summary>
        public async Task DeleteAll<T>(Expression<Func<T, bool>> criteria) where T : IEntity => await Delete(await GetList<T>(criteria));

        /// <summary>
        /// Updates all records in the database with the specified change.
        /// </summary>
        public async Task UpdateAll<T>(Action<T> change) where T : IEntity
        {
            var records = await GetList<T>();
            await Update(records, change);
        }
    }
}