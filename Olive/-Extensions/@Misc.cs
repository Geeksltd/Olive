using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// Provides extensions methods to Standard .NET types.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class OliveExtensions
    {
        const int MAXIMUM_ATTEMPTS = 3;
        const int ATTEMPT_PAUSE = 50 /*Milisseconds*/;

        /// <summary>
        /// Shortens this GUID.
        /// </summary>
        public static ShortGuid Shorten(this Guid guid) => new ShortGuid(guid);

        static Task<T> TryHardAsync<T>(FileSystemInfo fileOrFolder, Func<Task<T>> func, string error)
        {
            var resultTask = new TaskCompletionSource<T>();
            DoTryHardAsync(fileOrFolder, async () => resultTask.TrySetResult(await func()), error).GetAwaiter();
            return resultTask.Task;
        }

        static async Task DoTryHardAsync(FileSystemInfo fileOrFolder, Func<Task> func, string error)
        {
            var attempt = 0;

            Exception problem = null;

            while (attempt <= MAXIMUM_ATTEMPTS)
            {
                try
                {
                    await func?.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    problem = ex;

                    // Remove attributes:
                    try { fileOrFolder.Attributes = FileAttributes.Normal; }
                    catch
                    {
                        // No logging needed
                    }

                    attempt++;

                    // Pause for a short amount of time (to allow a potential external process to leave the file/directory).
                    await Task.Delay(ATTEMPT_PAUSE);
                }
            }

            throw new IOException(error.FormatWith(fileOrFolder.FullName), problem);
        }

        static T TryHard<T>(FileSystemInfo fileOrFolder, Func<T> action, string error)
        {
            var result = default(T);
            DoTryHard(fileOrFolder, delegate { result = action(); }, error);
            return result;
        }

        static void DoTryHard(FileSystemInfo fileOrFolder, Action action, string error)
        {
            var attempt = 0;

            Exception problem = null;

            while (attempt <= MAXIMUM_ATTEMPTS)
            {
                try
                {
                    action?.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    problem = ex;

                    // Remove attributes:
                    try { fileOrFolder.Attributes = FileAttributes.Normal; }
                    catch { }

                    attempt++;

                    // Pause for a short amount of time (to allow a potential external process to leave the file/directory).
                    System.Threading.Thread.Sleep(ATTEMPT_PAUSE);
                }
            }

            throw new IOException(error.FormatWith(fileOrFolder.FullName), problem);
        }

        /// <summary>
        /// Returns a nullable value wrapper object if this value is the default for its type.
        /// </summary>
        public static T? NullIfDefault<T>(this T @value, T defaultValue = default(T)) where T : struct
        {
            if (value.Equals(defaultValue)) return null;

            return @value;
        }

        public static DirectoryInfo WebsiteRoot(this AppDomain applicationDomain)
        {
            var root = applicationDomain.BaseDirectory.AsDirectory();
            if (root.Name.StartsWith("netcoreapp")) return root.Parent.Parent.Parent;
            else return root;
        }

        public static DirectoryInfo GetBaseDirectory(this AppDomain domain) => domain.BaseDirectory.AsDirectory();

        public static Assembly LoadAssembly(this AppDomain domain, string assemblyName)
        {
            var result = domain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName);
            if (result != null) return result;

            // Nothing found with exact name. Try with file name.
            var fileName = assemblyName.EnsureEndsWith(".dll", caseSensitive: false);

            var file = domain.GetBaseDirectory().GetFile(fileName);
            if (file.Exists())
                return Assembly.Load(AssemblyName.GetAssemblyName(file.FullName));

            // Maybe absolute file?
            if (File.Exists(fileName))
                return Assembly.Load(AssemblyName.GetAssemblyName(fileName));

            throw new Exception($"Failed to find the requrested assembly: '{assemblyName}'");
        }
    }
}