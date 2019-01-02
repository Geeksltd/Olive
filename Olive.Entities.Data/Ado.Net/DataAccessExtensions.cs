using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Olive.Entities.Data
{
    public static class DataAccessExtensions
    {
        static List<KeyValuePair<Type, ISqlCommandGenerator>> Pairs = new List<KeyValuePair<Type, ISqlCommandGenerator>>();

        public static IServiceCollection AddDataAccess<TConnection, TSqlCommandGenerator>(this IServiceCollection @this)
            where TConnection : DbConnection
            where TSqlCommandGenerator: ISqlCommandGenerator, new()
        {
            var pair = new KeyValuePair<Type, ISqlCommandGenerator>(typeof(TConnection), new TSqlCommandGenerator());

            Pairs.Add(pair);
            
            return @this;
        }

        public static IDatabase ConfigDataAccess(this IDatabase @this)
        {
            foreach (var pair in Pairs)
                DataAccess.Register(pair.Key, pair.Value, pair.Key.Namespace);

            return @this;
        }
    }

    public class DataAccessSettings
    {
        public ISqlCommandGenerator SqlCommandGenerator { get; set; }
    }
}
