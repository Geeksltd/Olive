using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Data access code for Application components.
    /// </summary>
    public partial class Database : IDatabase
    {
       public ICache Cache { get; private set; }

        /// <summary>
        /// Initialize instance of Database by injecting ICache dependency
        /// </summary>
        /// 
        bool IsSet(SaveBehaviour setting, SaveBehaviour behaviour) => (setting & behaviour) == behaviour;

        bool IsSet(DeleteBehaviour setting, DeleteBehaviour behaviour) => (setting & behaviour) == behaviour;

        bool NeedsTypeResolution(Type type) => type.IsInterface || type == typeof(Entity);

        public event AwaitableEventHandler CacheRefreshed;

        /// <summary>
        /// It's raised when any record is saved or deleted in the system.
        /// </summary>
        public event AwaitableEventHandler<IEntity> Updated;

        /// <summary>
        /// Clears the cache of all items.
        /// </summary>
        public async Task Refresh()
        {
            Cache.ClearAll();

            await CacheRefreshed.Raise();
        }

        public bool AnyOpenTransaction() => Transaction.Current != null || DbTransactionScope.Root != null;

        /// <summary>
        /// If there is an existing open transaction, it will simply run the specified action in it, Otherwise it will create a new transaction.
        /// </summary>
        public async Task EnlistOrCreateTransaction(Func<Task> func)
        {
            if (func == null) return;

            if (AnyOpenTransaction()) await func.Invoke();
            else
            {
                using (var scope = CreateTransactionScope())
                {
                    await func.Invoke();
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Returns the first record of the specified type of which ToString() would return the specified text .
        /// </summary>
        public async Task<T> Parse<T>(string toString, bool caseSensitive = false) where T : IEntity
        {
            // TODO: I have replaced StringComparison.InvariantCulture with StringComparison.Ordinal
            // It is possible to switch it back in next verions of .Net Core.
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            foreach (var instance in await GetList<T>())
            {
                string objectString;
                try { objectString = instance.ToString(); }
                catch (Exception ex)
                {
                    throw new Exception("Database.Parse() failed. Calling ToString() throw an exception on the {0} object with ID of '{1}'"
                        .FormatWith(typeof(T).Name, instance.GetId()), ex);
                }

                if (toString is null && objectString is null) return instance;

                if (toString is null || objectString is null) continue;

                if (objectString.Equals(toString, comparison)) return instance;
            }

            return default(T);
        }

        public async Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            return await GetProvider(instance).ReadManyToManyRelation(instance, property);
        }

        /// <summary>
        /// Gets a reloaded instance from the database to get a synced copy.
        /// </summary>
        public async Task<T> Reload<T>(T instance) where T : IEntity
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            Cache.Remove(instance);

            return (T)await Get(instance.GetId(), instance.GetType());
        }

        /// <summary>
        /// Determines if there is any object in the database of the specified type.
        /// </summary>
        public async Task<bool> Any<T>() where T : IEntity => await Of<T>().Count() > 0;

        /// <summary>
        /// Determines if there is any object in the database of the specified type matching a given criteria.
        /// </summary>
        public async Task<bool> Any<T>(Expression<Func<T, bool>> criteria) where T : IEntity =>
            await Of<T>().Where(criteria).Count() > 0;

        /// <summary>
        /// Determines whether there is no object of the specified type in the database.
        /// </summary>
        public async Task<bool> None<T>() where T : IEntity => !await Any<T>();

        /// <summary>
        /// Determines whether none of the objects in the database match a given criteria.
        /// </summary>
        public async Task<bool> None<T>(Expression<Func<T, bool>> criteria) where T : IEntity => !await Any<T>(criteria);
    }
}