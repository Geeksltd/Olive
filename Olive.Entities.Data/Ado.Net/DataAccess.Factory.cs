namespace Olive.Entities.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    partial class DataAccess
    {
        /// <summary>
        /// Keys are connection types such as SqlConnection, MySqlConnection, etc.
        /// </summary>
        static Dictionary<Type, ISqlCommandGenerator> CommandGenerators = new Dictionary<Type, ISqlCommandGenerator>();
        static Dictionary<string, Type> ConnectionTypes = new Dictionary<string, Type>();
        static Dictionary<Type, IParameterFactory> ParameterFactories = new Dictionary<Type, IParameterFactory>();

        // <summary>
        // Registgers a data Data Access instance for a specified provider type.
        // </summary>
        public static void Register(
            Type connectionType,
            ISqlCommandGenerator sqlCommandGenerator,
            IParameterFactory parameterFactory)
        {
            ConnectionTypes[connectionType.Name] = connectionType;
            CommandGenerators[connectionType] = sqlCommandGenerator;
            ParameterFactories[connectionType] = parameterFactory;
        }

        /// <summary>
        /// Gets the data accessor for the specified provider type.
        /// </summary>
        /// <param name="connectionType">For example SqlConnection, MySqlConnection, NpgsqlConnection, SqliteConnection, etc.
        /// If null or empty is specified, the first registered data access object will be returned.</param>
        public static IDataAccess Create(Type connectionType, string connectionString = null)
        {
            var dataAccessType = typeof(DataAccess<>).MakeGenericType(connectionType);

            if (!CommandGenerators.TryGetValue(connectionType, out var generator))
                throw new Exception("No data provider is registered in StartUp for " + connectionType.Name);

            ParameterFactories.TryGetValue(connectionType, out var parameterFactory);
            var config = Context.Current.GetService<IDatabaseProviderConfig>();

            return (IDataAccess)Activator.CreateInstance(dataAccessType, config, generator, connectionString, parameterFactory);
        }

        public static IDataAccess Create<TConnection>(string connectionString = null) where TConnection : IDbConnection
        {
            return Create(typeof(TConnection), connectionString);
        }

        /// <summary>
        /// Gets the data accessor for the specified provider type.
        /// </summary>
        /// <param name="connectionType">For example SqlConnection, MySqlConnection, NpgsqlConnection, SqliteConnection, etc.
        /// If null or empty is specified, the first registered data access object will be returned.</param>
        public static IDataAccess Create(string connectionType = null, string connectionString = null)
        {
            Type type;

            if (connectionType.IsEmpty())
            {
                type = ConnectionTypes.FirstOrDefault().Value ?? throw new Exception("No data provider is registered.");
            }
            else if (!ConnectionTypes.TryGetValue(connectionType, out type))
            {
                throw new Exception("No data provider is registered in StartUp for " + connectionType);
            }

            return Create(type, connectionString);
        }
    }
}