using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IDatabase
    {
        event AwaitableEventHandler CacheRefreshed;

        /// <summary>
        /// It's raised when any record is saved or deleted in the system.
        /// </summary>
        event AwaitableEventHandler<IEntity> Updated;

        Task Refresh();

        bool AnyOpenTransaction();

        Task EnlistOrCreateTransaction(Func<Task> action);

        Task<T> Parse<T>(string toString, bool caseSensitive = false) where T : IEntity;

        Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property);

        Task<T> Reload<T>(T instance) where T : IEntity;

        Task<bool> Any<T>() where T : IEntity;

        Task<bool> Any<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task<bool> None<T>() where T : IEntity;

        Task<bool> None<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task<int> Count<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        ICache Cache { get; }

        #region Delete

        Task Delete(IEntity instance);

        Task Delete(IEntity instance, DeleteBehaviour behaviour);

        Task Delete<T>(IEnumerable<T> instances) where T : IEntity;

        Task DeleteAll<T>() where T : IEntity;

        Task DeleteAll<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task UpdateAll<T>(Action<T> change) where T : IEntity;

        #endregion

        Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task<T> FirstOrDefault<T>() where T : IEntity;

        #region Get

        Task<T> Get<T>(string entityId) where T : IEntity;

        Task<T> Get<T>(Guid id) where T : IEntity;
        Task<T> Get<T>(Guid? id) where T : IEntity;

        Task<T> Get<T>(int? id) where T : IEntity<int>;
        Task<T> Get<T>(int id) where T : IEntity<int>;

        Task<T> Get<T>(long? id) where T : IEntity<long>;
        Task<T> Get<T>(long id) where T : IEntity<long>;

        Task<T> Get<T>(short? id) where T : IEntity<short>;
        Task<T> Get<T>(short id) where T : IEntity<short>;

        Task<T> Get<T>(byte? id) where T : IEntity<byte>;
        Task<T> Get<T>(byte id) where T : IEntity<byte>;

        Task<IEntity<Guid>> Get(Guid entityID, Type objectType);

        Task<IEntity> Get(object entityID, Type objectType);

        Task<T> GetOrDefault<T>(object id) where T : IEntity;

        Task<IEntity> GetOrDefault(object id, Type type);

        #endregion

        Task<IEnumerable<T>> GetList<T>(Expression<Func<T, bool>> criteria = null) where T : IEntity;

        ITransactionScope CreateTransactionScope(DbTransactionScopeOption option = DbTransactionScopeOption.Required);

        #region ProviderManagement

        IDataProvider GetProvider<T>() where T : IEntity;

        IDataProvider GetProvider(IEntity item);

        IDataProvider GetProvider(Type type);

        IDataAccess GetAccess(Type type);

        IDataAccess GetAccess<TEntity>() where TEntity : IEntity;

        IDataAccess GetAccess(string connectionString = null);

        #endregion

        #region Save

        /// <summary>
        /// Inserts or updates an object in the database.
        /// </summary>
        Task<T> Save<T>(T entity) where T : IEntity;

        /// <summary>
        /// Inserts or updates an object in the database.
        /// </summary>       
        Task Save(IEntity entity, SaveBehaviour behaviour);

        /// <summary>
        /// Saves the specified records in the data repository.
        /// The operation will run in a Transaction.
        /// </summary>
        Task<IEnumerable<T>> Save<T>(List<T> records) where T : IEntity;

        /// <summary>
        /// Runs an update command on a list of given objects and persists the updated objects in database.
        /// It returns the updated instances.
        /// </summary>
        /// <param name="items">The objects to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action) where T : IEntity;

        /// <summary>
        /// Runs an update command on a list of given objects and persists the updated objects in database.
        /// It returns the updated instances.
        /// </summary>
        /// <param name="items">The objects to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action, SaveBehaviour behaviour) where T : IEntity;

        /// <summary>
        /// Runs an update command on a given object's clone and persists the updated object in database. It returns the updated instance.
        /// </summary>
        /// <param name="item">The object to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        Task<T> Update<T>(T item, Action<T> action) where T : IEntity;

        /// <summary>
        /// Runs an update command on a given object's clone and persists the updated object in database. It returns the updated instance.
        /// </summary>
        /// <param name="item">The object to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.Property = "Value"</param>
        Task<T> Update<T>(T item, Action<T> action, SaveBehaviour behaviour) where T : IEntity;

        /// <summary>
        /// Runs an update command on a list of given objects and persists the updated objects in database.
        /// It returns the updated instances.
        /// </summary>
        /// <param name="items">The objects to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.someAsyncMethod()</param>
        Task<List<T>> Update<T>(IEnumerable<T> items, Func<T, Task> action) where T : IEntity;

        /// <summary>
        /// Runs an update command on a list of given objects and persists the updated objects in database.
        /// It returns the updated instances.
        /// </summary>
        /// <param name="items">The objects to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.someAsyncMethod()</param>
        Task<List<T>> Update<T>(IEnumerable<T> items, Func<T, Task> action, SaveBehaviour behaviour) where T : IEntity;

        /// <summary>
        /// Runs an update command on a given object's clone and persists the updated object in database. It returns the updated instance.
        /// </summary>
        /// <param name="item">The object to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.someAsyncMethod()</param>
        Task<T> Update<T>(T item, Func<T, Task> action) where T : IEntity;

        /// <summary>
        /// Runs an update command on a given object's clone and persists the updated object in database. It returns the updated instance.
        /// </summary>
        /// <param name="item">The object to be updated in database.</param>
        /// <param name="action">Update action. For example: o=>o.someAsyncMethod()</param>
        Task<T> Update<T>(T item, Func<T, Task> action, SaveBehaviour behaviour) where T : IEntity;

        /// <summary>
        /// Inserts the specified objects in bulk. None of the object events will be triggered.
        /// </summary>
        Task BulkInsert(Entity[] objects, int batchSize = 10, bool bypassValidation = false);

        /// <summary>
        /// Updates the specified objects in bulk. None of the object events will be triggered.
        /// </summary>
        Task BulkUpdate(Entity[] objects, int batchSize = 10, bool bypassValidation = false);

        #endregion

        #region Save List

        Task<IEnumerable<T>> Save<T>(T[] records) where T : IEntity;

        Task<IEnumerable<T>> Save<T>(IEnumerable<T> records) where T : IEntity;

        Task<IEnumerable<T>> Save<T>(IEnumerable<T> records, SaveBehaviour behaviour) where T : IEntity;

        #endregion

        IDatabaseQuery<TEntity> Of<TEntity>() where TEntity : IEntity;

        IDatabaseQuery Of(Type type);
    }
}
