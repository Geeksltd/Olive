using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class DataProviderFactory : IDataProviderFactory
    {
        static Dictionary<Type, DataProvider> ProviderCache = new Dictionary<Type, DataProvider>();
        readonly string ConnectionStringKey;
        readonly Type MappedType;
        readonly Assembly MappedAssembly;
        readonly IDataAccess DataAccess;

        public string ConnectionString { get; }

        public DataProviderFactory(Type type) : this(new DatabaseConfig.ProviderMapping { Type = type }) { }

        public DataProviderFactory(DatabaseConfig.ProviderMapping mapping)
        {
            DataAccess = mapping.CreateDataAccess();
            ConnectionString = mapping.ConnectionString;
            ConnectionStringKey = mapping.ConnectionStringKey;
            MappedType = mapping.GetMappedType();
            MappedAssembly = mapping.GetAssembly();
        }

        public IDataAccess GetAccess() => DataAccess;

        public IDataProvider GetProvider(Type type)
        {
            IDataProvider result = null;

            if (IsRelevant(type)) result = CreateProvider(type);
            else if (type.IsInterface) result = new InterfaceDataProvider(type);

            if (result == null)
                throw new NotSupportedException(type + " is not a data-supported type.");

            else if (ConnectionString.HasValue())
                result.ConnectionString = ConnectionString;

            else if (ConnectionStringKey.HasValue())
                result.ConnectionStringKey = ConnectionStringKey;

            return result;
        }

        IDataProvider CreateProvider(Type type)
        {
            return GetOrCreate(type, Context.Current.Database().Cache, DataAccess, DataAccess.GetSqlCommandGenerator());
        }

        protected virtual bool IsRelevant(Type type)
        {
            if (MappedType != null && MappedType != type) return false;
            if (MappedAssembly != type.Assembly) return false;
            if (TransientEntityAttribute.IsTransient(type)) return false;

            return true;
        }

        public virtual bool SupportsPolymorphism() => true;

        public static DataProvider GetOrCreate(Type type, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            lock (ProviderCache)
            {
                if (!ProviderCache.TryGetValue(type, out var result))
                {
                    result = new DataProvider(type, cache, access, sqlCommandGenerator);
                    result.Prepare();
                    ProviderCache.Add(type, result);
                }

                return result;
            }
        }
    }
}