using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Returns a DirectoryInfo object of the Website root directory.
        /// </summary>
        public static DirectoryInfo WebsiteRoot(this AppDomain @this)
        {
            var root = @this.BaseDirectory.AsDirectory();
            if (root.Name.StartsWith("netcoreapp")) return root.Parent.Parent.Parent;
            else return root;
        }

        /// <summary>
        /// Returns DirectoryInfo object of the base directory.
        /// </summary>
        public static DirectoryInfo GetBaseDirectory(this AppDomain @this) => @this.BaseDirectory.AsDirectory();

        /// <summary>
        /// loads an assembly given its name.
        /// </summary>
        public static Assembly LoadAssembly(this AppDomain @this, string assemblyName)
        {
            var result = @this.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName);
            if (result != null) return result;

            // Nothing found with exact name. Try with file name.
            var fileName = assemblyName.EnsureEndsWith(".dll", caseSensitive: false);

            var file = @this.GetBaseDirectory().GetFile(fileName);
            if (file.Exists())
                return Assembly.Load(AssemblyName.GetAssemblyName(file.FullName));

            // Maybe absolute file?
            if (File.Exists(fileName))
                return Assembly.Load(AssemblyName.GetAssemblyName(fileName));

            throw new Exception($"Failed to find the requrested assembly: '{assemblyName}'");
        }

        public static Type[] FindImplementers(this AppDomain @this, Type interfaceType) =>
            @this.FindImplementers(interfaceType, ignoreDrivedClasses: true);

        public static Type[] FindImplementers(this AppDomain @this, Type interfaceType, bool ignoreDrivedClasses)
        {
            var result = new List<Type>();

            foreach (var assembly in @this.GetAssemblies()
                .Where(a => a == interfaceType.Assembly || a.References(interfaceType.Assembly)))
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type == interfaceType) continue;
                        if (type.IsInterface) continue;

                        if (type.Implements(interfaceType))
                            result.Add(type);
                    }
                }
                catch
                {
                    // Can't load assembly. No logging is needed.
                }
            }

            // For any type, if it's parent is in the list, exclude it:
            if (ignoreDrivedClasses)
            {
                var typesWithParentsIn = result.Where(x => result.Contains(x.BaseType)).ToArray();

                foreach (var item in typesWithParentsIn)
                    result.Remove(item);
            }

            return result.ToArray();
        }
    }
}
