namespace Olive
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    partial class OliveExtensions
    {
        /// <summary>
        /// Gets the embedded resource name for a specified relative file path in the project.
        /// If the resulting resource name does not exist in this assembly it will throw.
        /// </summary>
        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static string GetEmbeddedResourceName(this Assembly @this, string rootNamespace, string fileRelativePath)
        {
            var result = rootNamespace + "." + fileRelativePath.Trim('/', '\\').Replace("/", "\\").Replace("\\", ".");

            using (var resource = @this.GetManifestResourceStream(result))
                if (resource == null)
                    throw new Exception($"The requested embedded resource '{result}' does not exist in the assembly '{@this.FullName}'");

            return result;
        }

        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static Task<byte[]> ReadEmbeddedResourceAsync(this Assembly @this, string rootNamespace, string fileRelativePath)
        {
            var resourceName = @this.GetEmbeddedResourceName(rootNamespace, fileRelativePath);
            return @this.ReadEmbeddedResourceAsync(resourceName);
        }

        public static async Task<byte[]> ReadEmbeddedResourceAsync(this Assembly @this, string resourceName)
        {
            try
            {
                using (var stream = @this.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("There is no embedded resource named '" + resourceName +
                       "' in the assembly: " + @this.FullName);

                    return await stream.ReadAllBytesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reading embedded resource failed: " + resourceName + Environment.NewLine + ex);
                Debug.WriteLine("Available resources:\r\n" + @this.GetManifestResourceNames().ToLinesString());
                throw;
            }
        }

        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static Task<string> ReadEmbeddedTextFileAsync(this Assembly @this, string rootNamespace, string fileRelativePath)
        {
            var resourceName = @this.GetEmbeddedResourceName(rootNamespace, fileRelativePath);
            return @this.ReadEmbeddedTextFileAsync(resourceName);
        }

        public static Task<string> ReadEmbeddedTextFileAsync(this Assembly @this, string resourceName)
        {
            return @this.ReadEmbeddedTextFileAsync(resourceName, Encoding.UTF8);
        }

        public static async Task<string> ReadEmbeddedTextFileAsync(this Assembly @this, string resourceName,
           Encoding encoding)
        {
            return encoding.GetString(await @this.ReadEmbeddedResourceAsync(resourceName));
        }

        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static byte[] ReadEmbeddedResource(this Assembly @this, string rootNamespace, string fileRelativePath)
        {
            var resourceName = @this.GetEmbeddedResourceName(rootNamespace, fileRelativePath);
            return @this.ReadEmbeddedResource(resourceName);
        }

        public static byte[] ReadEmbeddedResource(this Assembly @this, string resourceName)
        {
            try
            {
                using (var stream = @this.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("There is no embedded resource named '" + resourceName +
                       "' in the assembly: " + @this.FullName);

                    return stream.ReadAllBytes();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reading embedded resource failed: " + resourceName + Environment.NewLine + ex);
                Debug.WriteLine("Available resources:\r\n" + @this.GetManifestResourceNames().ToLinesString());
                throw;
            }
        }

        /// <param name="rootNamespace">The default namespace of your Visual Studio project.</param>
        /// <param name="fileRelativePath">For example MyRootFolder\MySubFolder\MyFile.cs (this is case sensitive).</param>
        public static string ReadEmbeddedTextFile(this Assembly @this, string rootNamespace, string fileRelativePath)
        {
            var resourceName = @this.GetEmbeddedResourceName(rootNamespace, fileRelativePath);
            return @this.ReadEmbeddedTextFile(resourceName);
        }

        public static string ReadEmbeddedTextFile(this Assembly @this, string resourceName)
        {
            return @this.ReadEmbeddedTextFile(resourceName, Encoding.UTF8);
        }

        public static string ReadEmbeddedTextFile(this Assembly @this, string resourceName,
           Encoding encoding)
        {
            return encoding.GetString(@this.ReadEmbeddedResource(resourceName));
        }
    }
}
