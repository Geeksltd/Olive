using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Olive.Entities.Data
{
    partial class DataAccess
    {
        /// <summary>
        /// Keys are data proidver types such as 
        /// </summary>
        static Dictionary<string, IDataAccess> Accessors = new Dictionary<string, IDataAccess>();

        // <summary>
        // Registgers a data Data Access instance for a specified provider type.
        // </summary>
        public static void Register(Type connectionType, ISqlCommandGenerator sqlCommandGenerator, string dataProviderType)
        {
            var dataAccessType = typeof(DataAccess<>).MakeGenericType(connectionType);

            Accessors[dataProviderType] = (IDataAccess)Activator.CreateInstance(dataAccessType, sqlCommandGenerator, null);
        }

        public static IDataAccess GetAccess<TConnection>(string connectionString = null)
            where TConnection : DbConnection, new()
        {
            var commandGenerator = Accessors.GetOrDefault(typeof(TConnection).Namespace)?.GetSqlCommandGenerator();
            if (commandGenerator == null)
                throw new Exception("No data provider is registered for " + typeof(TConnection).Namespace +
                    Environment.NewLine + "Consider setting it in Startup.cs using services.AddDataAccess(x => x....())");

            return new DataAccess<TConnection>(commandGenerator, connectionString);
        }

        /// <summary>
        /// Gets the data accessor for the specified provider type.
        /// </summary>
        /// <param name="dataProviderType">If null or empty is specified, the first registered data access object will be returned.</param>
        public static IDataAccess GetDataAccess(string dataProviderType = null)
        {
            if (dataProviderType.IsEmpty())
                return Accessors.FirstOrDefault().Value ?? throw new Exception("No data access is registered.");

            if (Accessors.TryGetValue(dataProviderType, out var result))
                return result;

            throw new Exception("No DataAccess is registered for provider type: " + dataProviderType);
        }
    }
}