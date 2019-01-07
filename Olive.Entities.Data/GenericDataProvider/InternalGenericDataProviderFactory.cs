using System;
using System.Collections.Generic;

namespace Olive.Entities.Data
{
    internal static class InternalDataProviderFactory
    {
        static Dictionary<Type, DataProvider> Cache = new Dictionary<Type, DataProvider>();

        public static DataProvider Get(Type type, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(type)) return Cache[type];

                var result = new DataProvider(type, cache, access, sqlCommandGenerator);
                result.Prepare();
                Cache.Add(type, result);

                return result;
            }
        }
    }
}