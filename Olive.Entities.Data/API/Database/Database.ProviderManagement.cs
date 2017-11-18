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
        static readonly Database instance = new Database();

        Database()
        {
            AssemblyProviderFactories = new Dictionary<Assembly, IDataProviderFactory>();
            TypeProviderFactories = new Dictionary<Type, IDataProviderFactory>();

            // Load from configuration:
            var bindResult = Config.Bind<DataProviderModelConfigurationSection>("DataProviderModel");
            var oliveData = (Olive.DataProviderModelConfigurationSection)bindResult;
            var configSection = new DataProviderModelConfigurationSection
            {
                Providers = new List<DataProviderFactoryInfo>(),
                FileDependancyPath = oliveData.FileDependancyPath,
                SyncFilePath = oliveData.SyncFilePath
            };
            foreach (var item in oliveData.Providers)
            {
                configSection.Providers.Add(new DataProviderFactoryInfo { Assembly = item.Assembly, AssemblyName = item.AssemblyName, ConnectionString = item.ConnectionString, ConnectionStringKey = item.ConnectionStringKey, MappingDirectory = item.MappingDirectory, MappingResource = item.MappingResource, ProviderFactoryType = item.ProviderFactoryType, Type = item.Type, TypeName = item.TypeName });
            }


            if (configSection != null)
            {
                if (configSection.Providers != null)
                    foreach (var factoryInfo in configSection.Providers)
                        RegisterDataProviderFactory(factoryInfo);
            }
        }

        public static Database Instance => instance;

        #region Updated event
        /// <summary>
        /// It's raised when any record is saved or deleted in the system.
        /// </summary>
        public AsyncEvent<IEntity> Updated { get; } = new AsyncEvent<IEntity>();

        Task OnUpdated(IEntity entity) => Updated.Raise(entity);

        #endregion

        object DataProviderSyncLock = new object();
        public void RegisterDataProviderFactory(DataProviderFactoryInfo factoryInfo)
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

        public Dictionary<Assembly, IDataProviderFactory> AssemblyProviderFactories { get; internal set; }
        Dictionary<Type, IDataProviderFactory> TypeProviderFactories;

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

        public IDataProvider GetProvider(Type type)
        {
            if (TypeProviderFactories.ContainsKey(type))
                return TypeProviderFactories[type].GetProvider(type);

            // Strange bug:
            if (AssemblyProviderFactories.Any(x => x.Key == null))
                AssemblyProviderFactories = new Dictionary<Assembly, IDataProviderFactory>();

            if (!AssemblyProviderFactories.ContainsKey(type.GetTypeInfo().Assembly))
                throw new InvalidOperationException("There is no registered 'data provider' for the assembly: " + type.GetTypeInfo().Assembly.FullName);

            return AssemblyProviderFactories[type.GetTypeInfo().Assembly].GetProvider(type);
        }

        /// <summary>
        /// Creates a transaction scope.
        /// </summary>
        public ITransactionScope CreateTransactionScope(DbTransactionScopeOption option = DbTransactionScopeOption.Required)
        {
            var isolationLevel = Config.Get("Default.Transaction.IsolationLevel", System.Data.IsolationLevel.Serializable);

            var typeName = Config.Get<string>("Default.TransactionScope.Type");

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