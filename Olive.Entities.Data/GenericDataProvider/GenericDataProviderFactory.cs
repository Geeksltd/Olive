using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Olive.Entities.Data
{
    public static class GenericDataProviderFactory<TConnection, TDataParameter>
             where TConnection : DbConnection, new()
             where TDataParameter : IDbDataParameter, new()
    {
        static Dictionary<Type, GenericDataProvider<TConnection, TDataParameter>> Cache = 
            new Dictionary<Type, GenericDataProvider<TConnection, TDataParameter>>();


        public static GenericDataProvider<TConnection, TDataParameter> Get(Type type, ICache cache, SqlCommandGenerator sqlCommandGenerator)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(type)) return Cache[type];

                var result = Create(type, cache, sqlCommandGenerator);
                Cache.Add(type, result);

                return result;
            }
        }

        static GenericDataProvider<TConnection, TDataParameter> Create(Type type, ICache cache, SqlCommandGenerator sqlCommandGenerator)
        {
            var resultType = typeof(GenericDataProvider<,>).MakeGenericType(typeof(TConnection), typeof(TDataParameter));

            return (GenericDataProvider<TConnection, TDataParameter>) Activator.CreateInstance(resultType, type, cache, sqlCommandGenerator);
        }
    }
}