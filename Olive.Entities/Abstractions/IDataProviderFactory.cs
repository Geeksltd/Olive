using System;

namespace Olive.Entities
{
    public interface IDataProviderFactory
    {
        /// <summary>
        /// Creates a data provider for the specified type.
        /// </summary>
        IDataProvider GetProvider(Type type);

        /// <summary>
        /// Determines whether this data provider factory handles interface data queries.
        /// </summary>
        bool SupportsPolymorphism();

        /// <summary>
        /// The default connection string used for its data providers.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets a data access instance used for executing database commands against its data source.
        /// </summary>
        IDataAccess GetAccess();
    }
}
