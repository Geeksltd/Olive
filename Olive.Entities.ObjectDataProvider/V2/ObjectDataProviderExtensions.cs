using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public static class ObjectDataProviderExtensions
    {
        public static ObjectDataProvider<TConnection, TDataParameter> GetProvider<TConnection, TDataParameter>(
                this DataProviderMetaData @this, ICache cache, SqlCommandGenerator sqlCommandGenerator)
            where TConnection : DbConnection, new()
            where TDataParameter : IDbDataParameter, new()
        {
            return GetProvider<TConnection, TDataParameter>(@this.Type, cache, sqlCommandGenerator);
        }            
        
        public static ObjectDataProvider<TConnection, TDataParameter> GetProvider<TConnection, TDataParameter>(
                this Type @this, ICache cache, SqlCommandGenerator sqlCommandGenerator)
            where TConnection : DbConnection, new()
            where TDataParameter : IDbDataParameter, new()
        {
            return ObjectDataProviderFactory<TConnection, TDataParameter>.Get(@this, cache, sqlCommandGenerator);
        }
    }
}
