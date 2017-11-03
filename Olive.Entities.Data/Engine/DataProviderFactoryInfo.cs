using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public class DataProviderFactoryInfo
    {
        string mappingDirectory;

        public string MappingResource { get; set; }

        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public string ProviderFactoryType { get; set; }

        public string MappingDirectory
        {
            get => mappingDirectory;
            set
            {
                if (value == null)
                    mappingDirectory = string.Empty;

                else if (value.StartsWith("\\\\") || value.Contains(":"))
                    // Absolute path:
                    mappingDirectory = value;

                else
                {
                    mappingDirectory = AppDomain.CurrentDomain.BaseDirectory + "/" + value + "/";
                    mappingDirectory = mappingDirectory.Replace("/", "\\");

                    mappingDirectory = mappingDirectory.KeepReplacing(@"\\", @"\");
                }
            }
        }

        public string ConnectionStringKey { get; set; }

        public string ConnectionString { get; set; }

        public Assembly Assembly { get; set; }
        public Type Type { get; set; }

        public Assembly GetAssembly()
        {
            if (Assembly == null)
                Assembly = AppDomain.CurrentDomain.LoadAssembly(AssemblyName);

            return Assembly;
        }

        public Type GetMappedType()
        {
            if (Type != null) return Type;

            if (TypeName.HasValue()) Type = GetAssembly().GetType(TypeName);

            return Type;
        }

        public async Task<string> LoadMappingXml()
        {
            if (MappingResource.IsEmpty())
                throw new Exception("No MappingResource is specified for this data provider factory.");

            if (MappingResource.Contains("/") || MappingResource.Contains("\\"))
            {
                // Physical file:
                var path = AppDomain.CurrentDomain.BaseDirectory + MappingResource;
                path = path.Replace("/", "\\").Replace("\\\\", "\\");

                if (path.StartsWith("\\")) path = "\\" + path;

                if (!File.Exists(path))
                    throw new FileNotFoundException("Could not find the data mapping xml at : " + path);

                return await File.ReadAllTextAsync(path);
            }
            else
            {
                // Embedded resource:
                foreach (var resourceName in GetAssembly().GetManifestResourceNames())
                {
                    if (resourceName.ToLower() == MappingResource.ToLower() || resourceName.ToLower().EndsWith("." + MappingResource.ToLower()))
                        return await LoadMappingText(resourceName);

                    // using (var resource = GetAssembly().GetManifestResourceStream(resourceName))
                    // {
                    //    using (var reader = new StreamReader(resource))
                    //    {
                    //        return reader.ReadToEnd();
                    //    }
                    // }
                }

                throw new Exception($"Could not build a data provider factory for {GetAssembly().FullName} because: " +
                    $"Could not load the manifest resource {MappingResource} from the assembly{GetAssembly().FullName}.");
            }
        }

        public async Task<string> LoadMappingText(string resourceName)
        {
            try
            {
                using (var resource = GetAssembly().GetManifestResourceStream(resourceName))
                {
                    using (var reader = new StreamReader(resource))
                        return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load the manifest resource text for '{0}'".FormatWith(resourceName), ex);
            }
        }
    }
}
