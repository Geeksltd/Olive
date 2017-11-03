namespace Olive
{
    using System;
    using System.Text;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    partial class OliveExtensions
    {
        /// <summary>
        /// Gets the embedded resource name for a specified relative file path in the project.
        /// If the resulting resource name does not exist in this assembly it will throw.
        /// </summary>
        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static string GetEmbeddedResourceName(this Assembly assembly, string rootNamespace, string fileRelativePath)
        {
            var result = rootNamespace + "." + fileRelativePath.Trim('/', '\\').Replace("/", "\\").Replace("\\", ".");

            using (var resource = assembly.GetManifestResourceStream(result))
                if (result == null)
                    throw new Exception($"The requested embedded resource '{result}' does not exist in the assembly '{assembly.FullName}'");

            return result;
        }

        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static Task<byte[]> ReadEmbeddedResource(this Assembly assembly, string rootNamespace, string fileRelativePath)
        {
            var resourceName = assembly.GetEmbeddedResourceName(rootNamespace, fileRelativePath);
            return assembly.ReadEmbeddedResource(resourceName);
        }

        public static async Task<byte[]> ReadEmbeddedResource(this Assembly assembly, string resourceName)
        {
            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("There is no embedded resource named '" + resourceName +
                       "' in the assembly: " + assembly.FullName);

                    return await stream.ReadAllBytes();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reading embedded resource failed: " + resourceName + Environment.NewLine + ex);
                Debug.WriteLine("Available resources:\r\n" + assembly.GetManifestResourceNames().ToLinesString());
                throw;
            }
        }

        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static Task<string> ReadEmbeddedTextFile(this Assembly assembly, string rootNamespace, string fileRelativePath)
        {
            var resourceName = assembly.GetEmbeddedResourceName(rootNamespace, fileRelativePath);
            return assembly.ReadEmbeddedTextFile(resourceName);
        }

        public static Task<string> ReadEmbeddedTextFile(this Assembly assembly, string resourceName)
        {
            return assembly.ReadEmbeddedTextFile(resourceName, Encoding.UTF8);
        }

        public static async Task<string> ReadEmbeddedTextFile(this Assembly assembly, string resourceName,
           Encoding encoding)
        {
            return encoding.GetString(await assembly.ReadEmbeddedResource(resourceName));
        }
    }
}