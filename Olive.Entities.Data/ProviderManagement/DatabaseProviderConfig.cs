using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.Entities.Data
{
    public interface IDatabaseProviderConfig
    {
        DatabaseConfig Configuration { get; }
        IDataProvider GetProvider(Type type);
        IDataProvider TryGetProvider(Type type);
        IDataAccess GetAccess(string connectionString = null);
        void RegisterDataProvider(Type entityType, IDataProvider dataProvider);
        List<IDataProvider> ResolveDataProviders(Type baseType);
        void Configure();
        IEnumerable<Assembly> GetRegisteredAssemblies();
    }

    public class DatabaseProviderConfig : IDatabaseProviderConfig
    {
        object DataProviderSyncLock = new object();

        Dictionary<Type, IDataProviderFactory> TypeProviderFactories = new Dictionary<Type, IDataProviderFactory>();
        Dictionary<Assembly, IDataProviderFactory> AssemblyProviderFactories = new Dictionary<Assembly, IDataProviderFactory>();

        Dictionary<Type, IDataProvider> TypeProviders = new Dictionary<Type, IDataProvider>();

        public DatabaseConfig Configuration { get; private set; }

        public void Configure()
        {
            if (Configuration == null)
                Config.Bind("Database", Configuration = new DatabaseConfig());

            foreach (var item in DataAccessOptions.Providers)
                DataAccess.Register(
                    item.ConnectionType,
                    item.SqlCommandGenerator,
                    item.ParameterFactory);

            foreach (var factoryInfo in Configuration.Providers.OrEmpty())
                RegisterDataProviderFactory(factoryInfo);
        }

        public void RegisterDataProviderFactory(DatabaseConfig.ProviderMapping factoryInfo)
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
            return TypeProviderFactories.Keys
                .Select(t => t.GetTypeInfo().Assembly)
                .Concat(AssemblyProviderFactories.Keys)
                .Distinct()
                .ToArray();
        }

        public IDataAccess GetAccess(string connectionString = null)
        {
            if (connectionString.IsEmpty()) connectionString = DataAccess.GetCurrentConnectionString();

            var factory = TypeProviderFactories.Values.Concat(AssemblyProviderFactories.Values)
                .FirstOrDefault(x => x.ConnectionString == connectionString);

            if (factory != null) return factory.GetAccess();

            if (connectionString.ToLowerOrEmpty() == DataAccess.GetCurrentConnectionString().ToLowerOrEmpty())
                return DataAccess.Create();

            throw new Exception("No data provider factory's connection string matched the specified connection string.");
        }

        IDataProviderFactory GetProviderFactory(Type type)
        {
            if (TypeProviderFactories.TryGetValue(type, out var factory)) return factory;

            if (AssemblyProviderFactories.TryGetValue(type.Assembly, out var result)) return result;

            if (!type.IsInterface)
                return new DataProviderFactory(type);

            return null;
        }

        public IDataProvider GetProvider(Type type)
        {
            return TryGetProvider(type)
                ?? throw new InvalidOperationException("There is no registered 'data provider' for the assembly: " +
                    type.GetTypeInfo().Assembly.FullName);
        }

        public IDataProvider TryGetProvider(Type type)
        {
            if (TypeProviders.TryGetValue(type, out var result))
                return result;

            var factory = GetProviderFactory(type);
            if (factory != null)
                lock (TypeProviders)
                {
                    try
                    {
                        return TypeProviders[type] = factory.GetProvider(type);
                    }
                    catch
                    {
                        return null;
                    }
                }

            if (type.IsInterface) return new InterfaceDataProvider(type);
            else return null;
        }

        public List<IDataProvider> ResolveDataProviders(Type baseType)
        {
            var factories = AssemblyProviderFactories
                .Where(f => f.Value.SupportsPolymorphism())
                .Where(f => f.Key.References(baseType.GetTypeInfo().Assembly))
                .ToList();

            var result = new List<IDataProvider>();

            foreach (var f in factories)
                result.Add(f.Value.GetProvider(baseType));

            foreach (var type in EntityFinder.FindPossibleTypes(baseType, mustFind: factories.None()))
                result.Add(GetProvider(type));

            return result;
        }
    }
}