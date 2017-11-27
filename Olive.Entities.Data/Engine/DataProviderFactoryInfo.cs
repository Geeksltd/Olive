using System;
using System.Reflection;

namespace Olive.Entities.Data
{
    public class DataProviderFactoryInfo
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
