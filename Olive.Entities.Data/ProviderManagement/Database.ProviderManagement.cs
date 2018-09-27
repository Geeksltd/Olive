using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace Olive.Entities.Data
{
    partial class Database
    {
        object DataProviderSyncLock = new object();

        [Obsolete("Use Context.Current.Database() instead.", error: true)]
        public static IDatabase Instance => Context.Current.Database();
        IConfiguration Config;

        public Dictionary<Assembly, IDataProviderFactory> AssemblyProviderFactories { get; }
            = new Dictionary<Assembly, IDataProviderFactory>();

        public Dictionary<Type, IDataProvider> TypeProviders { get; } = new Dictionary<Type, IDataProvider>();

        Dictionary<Type, IDataProviderFactory> TypeProviderFactories = new Dictionary<Type, IDataProviderFactory>();

        public static DatabaseConfig Configuration { get; private set; }

        public Database(IConfiguration config, ICache cache)
        {
            Config = config;
            Cache = cache;
        }

        public void Configure()
        {
            if (Configuration == null)
                Config.Bind("Database", Configuration = new DatabaseConfig());

            foreach (var factoryInfo in Configuration.Providers.OrEmpty())
                RegisterDataProviderFactory(factoryInfo);
        }

        #region Updated event
        /// <summary>
        /// It's raised when any record is saved or deleted in the system.
        /// </summary>
        public AsyncEvent<IEntity> Updated { get; } = new AsyncEvent<IEntity>();

        Task OnUpdated(IEntity entity) => Updated.Raise(entity);

        #endregion

        public void RegisterDataProviderFactory(DatabaseConfig.Provider factoryInfo)
        {
            if (factoryInfo == null) throw new ArgumentNullException(nameof(factoryInfo));

            lock (DataProviderSyncLock)
            {
                var type = factoryInfo.GetMappedType();
                var assembly = factoryInfo.GetAssembly();

                // var providerFactoryType = Type.GetType(factoryInfo.ProviderFactoryType); HAS A PROIBLEM WITH VERSIONING
                var providerFactoryType = assembly.GetTypes().FirstOrDefault(t => t.AssemblyQualifiedName == factoryInfo.ProviderFactoryType);
                if (providerFactoryType == null) providerFactoryType = assembly.GetType(factoryInfo.ProviderFactoryType);
                if (providerFactoryType == null) providerFactoryType = Type.GetType(factoryInfo.ProviderFactoryType);

                if (providerFactoryType == null)
                    throw new Exception("Could not find the type " + factoryInfo.ProviderFactoryType + " as specified in configuration.");

                var providerFactory = (IDataProviderFactory)Activator.CreateInstance(providerFactoryType, factoryInfo);

                if (type != null)
                {
                    TypeProviderFactories[type] = providerFactory;
                }
                else if (assembly != null && providerFactory != null)
                {
                    AssemblyProviderFactories[assembly] = providerFactory;
                }

                EntityFinder.ResetCache();
            }
        }

        public void RegisterDataProvider(Type entityType, IDataProvider dataProvider)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

            lock (TypeProviders)
                TypeProviders[entityType] = dataProvider;
        }

        /// <summary>
        /// Gets the assemblies for which a data provider factory has been registered in the current domain.
        /// </summary>
        public IEnumerable<Assembly> GetRegisteredAssemblies()
        {
            return TypeProviderFactories.Keys.Select(t => t.GetTypeInfo().Assembly).Concat(AssemblyProviderFactories.Keys).Distinct().ToArray();
        }

        public IDataProvider GetProvider<T>() where T : IEntity => GetProvider(typeof(T));

        public IDataAccess GetAccess(Type type) => GetProvider(type).Access;

        public IDataAccess GetAccess<TEntity>() where TEntity : IEntity => GetProvider<TEntity>().Access;

        public IDataAccess GetAccess(string connectionString = null)
        {
            if (connectionString.IsEmpty()) connectionString = DataAccess.GetCurrentConnectionString();

            var factory = TypeProviderFactories.Values
                 .Concat(AssemblyProviderFactories.Values)
                 .FirstOrDefault(x => x.ConnectionString == connectionString);

            if (factory == null)
                throw new Exception("No data provider factory's connection string matched the specified connection string.");

            return factory.GetAccess();
        }

        public IDataProvider GetProvider(IEntity item) => GetProvider(item.GetType());

        IDataProviderFactory GetProviderFactory(Type type)
        {
            if (TypeProviderFactories.TryGetValue(type, out var factory)) return factory;

            if (AssemblyProviderFactories.TryGetValue(type.Assembly, out var result)) return result;

            return null;
        }

        public IDataProvider GetProvider(Type type)
        {
            if (TypeProviders.TryGetValue(type, out var result))
                return result;

            var factory = GetProviderFactory(type);
            if (factory != null)
                lock (TypeProviders)
                    return TypeProviders[type] = factory.GetProvider(type);

            if (type.IsInterface) return new InterfaceDataProvider(type);
            else
                throw new InvalidOperationException("There is no registered 'data provider' for the assembly: " +
                    type.GetTypeInfo().Assembly.FullName);
        }

        /// <summary>
        /// Creates a transaction scope.
        /// </summary>
        public ITransactionScope CreateTransactionScope(DbTransactionScopeOption option = DbTransactionScopeOption.Required)
        {
            var isolationLevel = DbTransactionScope.GetDefaultIsolationLevel();

            var typeName = Configuration.Transaction.Type;

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

        List<IDataProvider> ResolveDataProviders(Type baseType)
        {
            var factories = AssemblyProviderFactories.Where(f => f.Value.SupportsPolymorphism() && f.Key.References(baseType.GetTypeInfo().Assembly)).ToList();

            var result = new List<IDataProvider>();

            foreach (var f in factories)
                result.Add(f.Value.GetProvider(baseType));

            foreach (var type in EntityFinder.FindPossibleTypes(baseType, mustFind: factories.None()))
                result.Add(GetProvider(type));

            return result;
        }
    }
}