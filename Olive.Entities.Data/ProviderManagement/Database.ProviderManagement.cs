using System;
using System.Data.Common;
using System.Transactions;

namespace Olive.Entities.Data
{
    partial class Database
    {
        [Obsolete("Use Context.Current.Database() instead.", error: true)]
        public static IDatabase Instance => Context.Current.Database();

        readonly Audit.IAudit Audit;
        readonly IDatabaseProviderConfig ProviderConfig;

        public Database(Audit.IAudit audit, IDatabaseProviderConfig providerConfig, ICache cache)
        {
            Audit = audit;
            ProviderConfig = providerConfig;
            Cache = cache;
        }

        public IDataProvider GetProvider(Type type) => ProviderConfig.GetProvider(type);

        public IDataProvider GetProvider(IEntity item) => GetProvider(item.GetType());

        public IDataProvider GetProvider<T>() where T : IEntity => GetProvider(typeof(T));

        public IDataAccess GetAccess(Type type) => GetProvider(type).Access;

        public IDataAccess GetAccess<TEntity>() where TEntity : IEntity => GetProvider<TEntity>().Access;

        public IDataAccess GetAccess<TConnection>(string connectionString = null) where TConnection : DbConnection, new()
        {
            return DataAccess.Create<TConnection>(connectionString);
        }

        public IDataAccess GetAccess(string connectionString = null) => ProviderConfig.GetAccess(connectionString);


        /// <summary>
        /// Creates a transaction scope.
        /// </summary>
        public ITransactionScope CreateTransactionScope(DbTransactionScopeOption option = DbTransactionScopeOption.Required)
        {
            var isolationLevel = DbTransactionScope.GetDefaultIsolationLevel();

            var typeName = ProviderConfig.Configuration.Transaction.Type;

            if (typeName.HasValue())
            {
                Type type = null; // this is a workaround.
                var dummy = typeof(DbTransactionScope);
                if (dummy.AssemblyQualifiedName.StartsWith(typeName))
                    type = dummy;
                else
                    type = Type.GetType(typeName);

                if (type == null) throw new Exception("Cannot load type: " + typeName);

                return (ITransactionScope)type.CreateInstance(isolationLevel, option);
            }

            // Fall back to TransactionScope:
            var oldOption = option.ToString().To<TransactionScopeOption>();
            return new TransactionScopeWrapper(isolationLevel.ToString().To<IsolationLevel>().CreateScope(oldOption));
        }
    }
}