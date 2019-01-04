using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Olive.Entities.Data
{
    partial class DataAccess
    {
        static Dictionary<string, IDataAccess> Accessors = new Dictionary<string, IDataAccess>();

        public static void Register<TConnection>(ISqlCommandGenerator sqlCommandGenerator, string dataProviderType)
            where TConnection : DbConnection, new()
        {
            Accessors[dataProviderType] = new DataAccess<TConnection>(sqlCommandGenerator);
        }

        public static void Register(Type connectionType, ISqlCommandGenerator sqlCommandGenerator, string dataProviderType)
        {
            var dataAccessType = typeof(DataAccess<>).MakeGenericType(connectionType);

            Accessors[dataProviderType] = (IDataAccess)Activator.CreateInstance(dataAccessType, sqlCommandGenerator, null);
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