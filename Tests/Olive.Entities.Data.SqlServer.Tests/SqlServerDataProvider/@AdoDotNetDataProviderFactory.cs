namespace AppData
{
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    using Olive.Entities.Data.SqlServer.Tests;
    using System;
    using System.Data.SqlClient;

    /// <summary>A factory that can instantiate Data Provider objects for MS.MindMap</summary>
    [EscapeGCop("Auto generated code.")]
    public class AdoDotNetDataProviderFactory : IDataProviderFactory
    {
        string ConnectionStringKey;
        public string ConnectionString { get; private set; }

        /// <summary>Initializes a new instance of AdoDotNetDataProviderFactory.</summary>
        public AdoDotNetDataProviderFactory(DatabaseConfig.Provider provider)
        {
            ConnectionString = provider.ConnectionString.Or(DataAccess.GetCurrentConnectionString());
            ConnectionStringKey = provider.ConnectionStringKey;
        }

        public IDataAccess GetAccess() => new DataAccess<SqlConnection>();

        /// <summary>Gets a data provider instance for the specified entity type.</summary>
        public virtual IDataProvider GetProvider(Type type)
        {
            IDataProvider result = null;

            if (type == typeof(Person)) result = new PersonDataProvider();
            else if (type.IsInterface) result = new InterfaceDataProvider(type);

            if (result == null)
            {
                throw new NotSupportedException(type + " is not a data-supported type.");
            }
            else if (ConnectionString.HasValue())
            {
                result.ConnectionString = ConnectionString;
            }
            else if (ConnectionStringKey.HasValue())
            {
                result.ConnectionStringKey = ConnectionStringKey;
            }

            return result;
        }

        /// <summary>Determines whether this data provider factory handles interface data queries.</summary>
        public virtual bool SupportsPolymorphism() => true;
    }
}