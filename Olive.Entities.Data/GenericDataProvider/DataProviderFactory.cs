using System;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class DataProviderFactory : IDataProviderFactory
    {
        readonly string ConnectionStringKey;
        readonly Type MappedType;
        readonly Assembly MappedAssembly;
        readonly IDataAccess DataAccess;

        public string ConnectionString { get; }

        public DataProviderFactory(Type type) : this(new DatabaseConfig.ProviderMapping { Type = type })
        {
        }

        public DataProviderFactory(DatabaseConfig.ProviderMapping mapping)
        {
            DataAccess = mapping.GetDataAccess();
            ConnectionString = mapping.ConnectionString.Or(Data.DataAccess.GetCurrentConnectionString());
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
            return InternalDataProviderFactory.Get(type, Context.Current.Cache(), DataAccess, DataAccess.GetSqlCommandGenerator());
        }

        protected virtual bool IsRelevant(Type type)
        {
            if (MappedType != null && MappedType != type) return false;
            if (MappedAssembly != type.Assembly) return false;
            if (TransientEntityAttribute.IsTransient(type)) return false;

            return true;
        }

        public virtual bool SupportsPolymorphism() => true;
    }
}
