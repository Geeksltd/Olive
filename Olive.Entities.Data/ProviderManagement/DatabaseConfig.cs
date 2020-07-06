using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class DatabaseConfig
    {
        public List<ProviderMapping> Providers { get; set; }
        public bool Profile { get; set; }
        public CacheConfig Cache { get; set; }
        public TransactionConfig Transaction { get; set; }

        public class CacheConfig
        {
            public bool ConcurrencyAware { get; set; }
            public bool Enabled { get; set; }
            public bool PerRequest { get; set; }

            public string Mode
            {
                get
                {
                    if (!Enabled) return "off";
                    if (PerRequest) return "multi-server";
                    return "single-server";
                }
                set
                {
                    switch (value.ToLowerOrEmpty())
                    {
                        case "off": Enabled = false; break;
                        case "single-server": Enabled = true; PerRequest = false; break;
                        case "multi-server": Enabled = true; PerRequest = true; break;
                        default: throw new NotSupportedException(value + " is not a supported value for Cache mode.");
                    }
                }
            }
        }

        public class TransactionConfig
        {
            public string Type { get; set; }
            public bool EnforceForSave { get; set; }
        }

        public class ProviderMapping
        {
            public string AssemblyName { get; set; }
            public string TypeName { get; set; }
            public string ProviderFactoryType { get; set; } = "Olive.Entities.Data.DataProviderFactory, Olive.Entities.Data";
            public string ConnectionStringKey { get; set; }
            public string ConnectionString { get; set; }

            public string SqlClient { get; set; }

            public Assembly Assembly { get; set; }
            public Type Type { get; set; }

            public Assembly GetAssembly()
            {
                if (Assembly != null) return Assembly;
                if (AssemblyName.HasValue()) return Assembly = AppDomain.CurrentDomain.LoadAssembly(AssemblyName);
                if (Type != null) return Assembly = Type.Assembly;
                return null;
            }

            public Type GetMappedType()
            {
                if (Type != null) return Type;
                if (TypeName.HasValue()) Type = GetAssembly().GetType(TypeName);
                return Type;
            }

            internal IDataAccess CreateDataAccess()
            {
                if (ConnectionString.IsEmpty() && ConnectionStringKey.HasValue())
                    ConnectionString = Context.Current.GetService<IConnectionStringProvider>()
                        .GetConnectionString(ConnectionStringKey);

                return DataAccess.Create(SqlClient, ConnectionString);
            }
        }
    }
}