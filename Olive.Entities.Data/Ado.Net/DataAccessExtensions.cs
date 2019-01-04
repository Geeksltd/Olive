using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Olive.Entities.Data
{
    public class DataAccessOptions
    {
        internal static List<KeyValuePair<Type, ISqlCommandGenerator>> Providers = new List<KeyValuePair<Type, ISqlCommandGenerator>>();

        public DataAccessOptions Add<TConnection, TSqlCommandGenerator>()
           where TConnection : DbConnection
           where TSqlCommandGenerator : ISqlCommandGenerator, new()
        {
            var pair = new KeyValuePair<Type, ISqlCommandGenerator>(typeof(TConnection), new TSqlCommandGenerator());

            Providers.Add(pair);

            return this;
        }
    }

    public static class DataAccessExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection @this, Action<DataAccessOptions> options)
        {
            options(new DataAccessOptions());
            return @this;
        }

        public static IDatabase ConfigDataAccess(this IDatabase @this)
        {
            foreach (var pair in DataAccessOptions.Providers)
                DataAccess.Register(pair.Key, pair.Value, pair.Key.Namespace);

            return @this;
        }
    }
}