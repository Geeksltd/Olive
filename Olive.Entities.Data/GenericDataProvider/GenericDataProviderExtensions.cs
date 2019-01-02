using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Olive.Entities.Data
{
    public static class DataProviderExtensions
    {
        public static DataProvider GetProvider(
                this IDataProviderMetaData @this, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            return GetProvider(@this.Type, cache, access, sqlCommandGenerator);
        }            
        
        public static DataProvider GetProvider(
                this Type @this, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            return InternalDataProviderFactory.Get(@this, cache, access, sqlCommandGenerator);
        }
    }
}
