using System;

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