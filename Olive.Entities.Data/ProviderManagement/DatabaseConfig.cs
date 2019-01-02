using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class DatabaseConfig
    {
        public List<ProviderMapping> ProviderMappings { get; set; }
        public bool Profile { get; set; }
        public CacheConfig Cache { get; set; }
        public TransactionConfig Transaction { get; set; }

        public class CacheConfig
        {
            public bool ConcurrencyAware { get; set; }
            public bool Enabled { get; set; }
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
            public string ProviderFactoryType { get; set; }
            public string ConnectionStringKey { get; set; }
            public string ConnectionString { get; set; }

            public string SqlClient { get; set; } = "System.Data.SqlClient";

            public Assembly Assembly { get; set; }
            public Type Type { get; set; }

            public Assembly GetAssembly()
                => Assembly ?? (Assembly = AppDomain.CurrentDomain.LoadAssembly(AssemblyName));

            public Type GetMappedType()
            {
                if (Type != null) return Type;
                if (TypeName.HasValue()) Type = GetAssembly().GetType(TypeName);
                return Type;
            }

            internal IDataAccess GetDataAccess() => DataAccess.GetDataAccess(SqlClient);
        }
    }
}