using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IDatabase
    {
        AsyncEvent CacheRefreshed { get; }

        /// <summary>
        /// It's raised when any record is saved or deleted in the system.
        /// </summary>
        AsyncEvent<IEntity> Updated { get; }

        Task Refresh();

        bool AnyOpenTransaction();

        Task EnlistOrCreateTransaction(Func<Task> action);

        Task<T> Parse<T>(string toString, bool caseSensitive = false) where T : IEntity;

        int CountAllObjectsInCache();

        Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property);

        Task<T> Reload<T>(T instance) where T : IEntity;

        Task<bool> Any<T>() where T : IEntity;

        Task<bool> Any<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task<bool> None<T>() where T : IEntity;

        Task<bool> None<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task<int> Count<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        #region Delete

        Task Delete(IEntity instance);

        Task Delete(IEntity instance, DeleteBehaviour behaviour);

        Task Delete<T>(IEnumerable<T> instances) where T : IEntity;

        Task DeleteAll<T>() where T : IEntity;

        Task DeleteAll<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        Task UpdateAll<T>(Action<T> change) where T : IEntity;

        #endregion

        Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> criteria) where T : IEntity;

        #region Get

        Task<T> Get<T>(string entityId) where T : IEntity;

        Task<T> Get<T>(Guid id) where T : IEntity;

        Task<T> Get<T>(Guid? id) where T : IEntity;

        Task<T> Get<T>(int? id) where T : IEntity<int>;

        Task<T> Get<T>(int id) where T : IEntity<int>;

        Task<IEntity<Guid>> Get(Guid entityID, Type objectType);

        Task<IEntity> Get(object entityID, Type objectType);

        Task<T> GetOrDefault<T>(object id) where T : IEntity;

        Task<IEntity> GetOrDefault(object id, Type type);

        #endregion

        Task<IEnumerable<T>> GetList<T>(Expression<Func<T, bool>> criteria = null) where T : IEntity;

        ITransactionScope CreateTransactionScope(DbTransactionScopeOption option = DbTransactionScopeOption.Required);

        #region ProviderManagement

        Dictionary<System.Reflection.Assembly, IDataProviderFactory> AssemblyProviderFactories { get; }

        IEnumerable<System.Reflection.Assembly> GetRegisteredAssemblies();

        IDataProvider GetProvider<T>() where T : IEntity;

        IDataProvider GetProvider(IEntity item);

        IDataProvider GetProvider(Type type);

        IDataAccess GetAccess(Type type);

        IDataAccess GetAccess<TEntity>() where TEntity : IEntity;

        IDataAccess GetAccess(string connectionString = null);

        #endregion

        #region Save

        Task<T> Save<T>(T entity) where T : IEntity;

        Task Save(IEntity entity, SaveBehaviour behaviour);

        Task<IEnumerable<T>> Save<T>(List<T> records) where T : IEntity;

        Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action) where T : IEntity;

        Task<List<T>> Update<T>(IEnumerable<T> items, Action<T> action, SaveBehaviour behaviour) where T : IEntity;

        Task<T> Update<T>(T item, Action<T> action) where T : IEntity;

        Task<T> Update<T>(T item, Action<T> action, SaveBehaviour behaviour) where T : IEntity;

        Task BulkInsert(Entity[] objects, int batchSize = 10, bool bypassValidation = false);

        Task BulkUpdate(Entity[] objects, int batchSize = 10, bool bypassValidation = false);

        #endregion

        #region Save List

        Task<IEnumerable<T>> Save<T>(T[] records) where T : IEntity;

        Task<IEnumerable<T>> Save<T>(IEnumerable<T> records) where T : IEntity;

        Task<IEnumerable<T>> Save<T>(IEnumerable<T> records, SaveBehaviour behaviour) where T : IEntity;

        #endregion

        IDatabaseQuery<TEntity> Of<TEntity>() where TEntity : IEntity;

        IDatabaseQuery Of(Type type);

        void RegisterDataProvider(Type entityType, IDataProvider dataProvider);
    }
}
