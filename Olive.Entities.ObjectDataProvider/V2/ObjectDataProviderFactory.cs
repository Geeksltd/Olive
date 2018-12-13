using System;
using System.Data;
using System.Data.Common;

namespace Olive.Entities.ObjectDataProvider.V2
{
    internal class ObjectDataProviderFactory
    {
        internal static ObjectDataProvider<TConnection, TDataParameter> Get<TConnection, TDataParameter>(Type type)
             where TConnection : DbConnection, new()
             where TDataParameter : IDbDataParameter, new()
        {
            throw new NotImplementedException();
        }
    }
}