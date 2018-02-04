using System;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class DatabaseConfig
    {
        public List<Provider> Providers;
        public bool Profile;
        public CacheConfig Cache;
        public TransactionConfig Transaction;

        public class CacheConfig
        {
            public bool ConcurrencyAware, Enabled;
        }

        public class TransactionConfig
        {
            public string Type;
            public bool EnforceForSave;
        }

        public class Provider
        {
            public string AssemblyName { get; set; }
            public string TypeName { get; set; }
            public string ProviderFactoryType { get; set; }
            public string ConnectionStringKey { get; set; }
            public string ConnectionString { get; set; }

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
        }
    }
}