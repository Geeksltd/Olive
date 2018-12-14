using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public static class ObjectDataProviderFactory<TConnection, TDataParameter>
             where TConnection : DbConnection, new()
             where TDataParameter : IDbDataParameter, new()
    {
        static Dictionary<Type, ObjectDataProvider<TConnection, TDataParameter>> Cache = 
            new Dictionary<Type, ObjectDataProvider<TConnection, TDataParameter>>();


        public static ObjectDataProvider<TConnection, TDataParameter> Get(Type type, ICache cache, SqlCommandGenerator sqlCommandGenerator)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(type)) return Cache[type];

                var result = Create(type, cache, sqlCommandGenerator);
                Cache.Add(type, result);

                return result;
            }
        }

        static ObjectDataProvider<TConnection, TDataParameter> Create(Type type, ICache cache, SqlCommandGenerator sqlCommandGenerator)
        {
            var resultType = typeof(ObjectDataProvider<,>).MakeGenericType(typeof(TConnection), typeof(TDataParameter));

            return (ObjectDataProvider<TConnection, TDataParameter>) Activator.CreateInstance(resultType, type, cache, sqlCommandGenerator);
        }
    }
}