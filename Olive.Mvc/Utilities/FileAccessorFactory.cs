using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Olive.Entities;
using Olive.Entities.Data;

namespace Olive.Mvc
{
    public class FileAccessorFactory : IFileAccessorFactory
    {
        IDatabaseProviderConfig ProviderConfig;
        readonly IDatabase Database;

        public FileAccessorFactory(IDatabaseProviderConfig providerConfig, IDatabase database)
        {
            ProviderConfig = providerConfig;
            Database = database;
        }

        public async Task<FileAccessor> Create(string path, IPrincipal currentUser)
        {
            var temp = GetTypeAndProperty(path);

            var result = new FileAccessor(temp.Type, temp.PropertyInfo, temp.Id, currentUser, Database);

            await result.LoadBlob();

            return result;
        }

        protected virtual (Type Type, PropertyInfo PropertyInfo, string Id) GetTypeAndProperty(string path)
        {
            var pathParts = path.Split('/');

            if (pathParts.Length < 2)
                throw new Exception($"Invalid path specified: '{path}'");

            var typeName = pathParts[0].Split('.')[0];

            var type = FindType(typeName);

            if (type == null) throw new Exception($"Invalid type name specified: '{typeName}'");

            var property = pathParts[0].Split('.')[1];

            var propertyInfo = type.GetProperty(property);
            if (propertyInfo == null)
                throw new Exception($"Could not find the property '{property}' on the type '{type.FullName}'.");

            return (type, propertyInfo, pathParts[1]);
        }

        private Type FindType(string typeName)
        {
            var assemblies = ProviderConfig.GetRegisteredAssemblies().ToList();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetExportedTypes().SingleOrDefault(t => t.Name == typeName);
                if (type != null)
                    return type;

            }
            var interfaceType = typeof(IEntity);
            assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a == interfaceType.Assembly || a.References(interfaceType.Assembly))
                .OrderBy(a => !a.FullName.Contains("Domain"))
                .ToList();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var type = assembly.GetTypes().FirstOrDefault(t => t.Implements(interfaceType) && t.Name == typeName);
                    if (type != null)
                        return type;
                }
                catch (Exception ex)
                {
                    Log.For(typeof(OliveExtensions))
                        .Info($"Could not load assembly {assembly.FullName} because: {ex.Message}");
                }
            }
            return null;
        }
    }
}