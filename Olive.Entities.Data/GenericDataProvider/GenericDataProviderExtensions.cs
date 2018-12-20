using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Olive.Entities.Data
{
    public static class GenericDataProviderExtensions
    {
        public static GenericDataProvider<TConnection, TDataParameter> GetProvider<TConnection, TDataParameter>(
                this DataProviderMetaData @this, ICache cache, SqlCommandGenerator sqlCommandGenerator)
            where TConnection : DbConnection, new()
            where TDataParameter : IDbDataParameter, new()
        {
            return GetProvider<TConnection, TDataParameter>(@this.Type, cache, sqlCommandGenerator);
        }            
        
        public static GenericDataProvider<TConnection, TDataParameter> GetProvider<TConnection, TDataParameter>(
                this Type @this, ICache cache, SqlCommandGenerator sqlCommandGenerator)
            where TConnection : DbConnection, new()
            where TDataParameter : IDbDataParameter, new()
        {
            return GenericDataProviderFactory<TConnection, TDataParameter>.Get(@this, cache, sqlCommandGenerator);
        }
    }
}
