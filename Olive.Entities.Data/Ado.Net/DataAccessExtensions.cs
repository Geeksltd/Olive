using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Olive.Entities.Data
{
    public class DataAccessOptions
    {
        internal static List<(Type ConnectionType, ISqlCommandGenerator SqlCommandGenerator, IParameterFactory ParameterFactory)>
            Providers = new List<(Type, ISqlCommandGenerator, IParameterFactory)>();

        public DataAccessOptions Add<TConnection, TSqlCommandGenerator>(IParameterFactory parameterFactory = null)
           where TConnection : DbConnection
           where TSqlCommandGenerator : ISqlCommandGenerator, new()
        {
            Providers.Add((typeof(TConnection), new TSqlCommandGenerator(), parameterFactory));
            return this;
        }
    }

    public static class DataAccessExtensions
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection @this, Action<DataAccessOptions> options)
        {
            @this.TryAddTransient<IConnectionStringProvider, ConnectionStringProvider>();

            options(new DataAccessOptions());
            return @this;
        }

        public static ICache Cache(this IDatabase database) => database.Cache;
    }
}