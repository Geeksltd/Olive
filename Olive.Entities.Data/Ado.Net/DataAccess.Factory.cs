using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        public static IDataAccess GetDataAccess(string dataProviderType)
        {
            if (Accessors.TryGetValue(dataProviderType, out var result))
                return result;

            throw new Exception("No DataAccess is registered for provider type: " + dataProviderType);
        }
    }
}