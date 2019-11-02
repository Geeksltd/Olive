using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// If specified as recursive and harshly, then it tries multiple times to delete this directory.        
        /// </summary>
        public static async Task DeleteAsync(this DirectoryInfo @this, bool recursive, bool harshly)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (!@this.Exists()) return;

            if (harshly && !recursive)
                throw new ArgumentException("For deleting a folder harshly, the recursive option should also be specified.");

            if (!harshly)
            {
                @this.Delete(recursive);
                return;
            }

            // Otherwise, it is harsh and recursive:
            try
            {
                // First attempt: Simple delete:
                @this.Delete(recursive: true);
            }
            catch
            {
                // No loging is needed
                // Normal attempt failed. Let's try it harshly!
                await HarshDelete(@this);
            }
        }

        public static void DeleteIfExists(this DirectoryInfo @this, bool recursive = false)
        {
            if (@this == null) return;
            if (!@this.Exists()) return;
            @this.Delete(recursive);
        }

        /// <summary>
        /// Will try to delete a specified directory by first deleting its sub-folders and files.
        /// </summary>
        static async Task HarshDelete(DirectoryInfo directory)
        {
            if (!directory.Exists()) return;

            await DoTryHardAsync(directory, async () =>
            {
                await directory.GetFiles().Do(async (f) => await f.DeleteAsync(harshly: true));
                await directory.GetDirectories().Do(async (d) => await HarshDelete(d));
                await Task.Factory.StartNew(directory.Delete);
            }, "The system cannot delete the directory, even after several attempts. Directory: {0}");
        }

        /// <summary>
        /// Copies the entire content of a directory to a specified destination.
        /// </summary>
        public static Task CopyToAsync(this DirectoryInfo @this, DirectoryInfo destination, bool overwrite = false)
        {
            return CopyToAsync(@this, destination.FullName, overwrite);
        }

        /// <summary>
        /// Determines whether the file's contents start with MZ which is the signature for EXE files.
        /// </summary>
        public static bool HasExeContent(this FileInfo @this)
        {
            var twoBytes = new byte[2];
            using (var fileStream = File.Open(@this.FullName, FileMode.Open))
            {
                try
                {
                    fileStream.Read(twoBytes, 0, 2);
                }
                catch
                {
                    // No logging is needed
                    return false; // No content
                }
            }

            return Encoding.UTF8.GetString(twoBytes) == "MZ";
        }

        /// <summary>
        /// Copies the entire content of a directory to a specified destination.
        /// </summary>
        public static async Task CopyToAsync(this DirectoryInfo @this, string destination, bool overwrite = false)
        {
            destination.AsDirectory().EnsureExists();

            foreach (var file in @this.GetFiles())
                await file.CopyToAsync(Path.Combine(destination, file.Name).AsFile(), overwrite);

            foreach (var sub in @this.GetDirectories())
                await sub.CopyToAsync(Path.Combine(destination, sub.Name), overwrite);
        }

        /// <summary>
        /// Copies the entire content of a directory to a specified destination.
        /// </summary>
        public static void CopyTo(this DirectoryInfo @this, string destination, bool overwrite = false)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            @this.ExistsOrThrow();

            destination.AsDirectory().EnsureExists();

            foreach (var file in @this.GetFiles())
                file.CopyTo(Path.Combine(destination, file.Name).AsFile(), overwrite);

            foreach (var sub in @this.GetDirectories())
                sub.CopyTo(Path.Combine(destination, sub.Name), overwrite);
        }

        /// <summary>
        /// Copies this file to a specified destination directiry with the original file name.
        /// </summary>
        public static async Task CopyTo(this FileInfo @this, DirectoryInfo destinationDirectory, bool overwrite = false) =>
            await @this.CopyToAsync(destinationDirectory.GetFile(@this.Name), overwrite);

        public static string[] GetFiles(this DirectoryInfo @this, bool includeSubDirectories)
        {
            var result = new List<string>(@this.GetFiles().Select(f => f.FullName));

            if (includeSubDirectories)
                foreach (var subFolder in @this.GetDirectories())
                    result.AddRange(subFolder.GetFiles(includeSubDirectories: true));

            return result.ToArray();
        }

        /// <summary>
        /// Gets a file info with the specified name under this folder. That file does not have to exist already.
        /// </summary>
        public static FileInfo GetFile(this DirectoryInfo folder, string fileName) => folder.PathCombine(fileName).AsFile();

        /// <summary>
        /// Gets a subdirectory with the specified name. It does not need to exist necessarily.
        /// </summary>
        public static DirectoryInfo GetSubDirectory(this DirectoryInfo parent, string subdirectoryName)
        {
            if (subdirectoryName.IsEmpty())
                throw new ArgumentNullException("GetSubDirectory(name) expects a non-empty sub-directory name.");

            return parent.PathCombine(subdirectoryName).AsDirectory();
        }

        public static string PathCombine(this DirectoryInfo parent, string subdirectoryName)
        {
            var parts = new[] { parent.FullName }.Concat(subdirectoryName.OrEmpty().Split('\\', '/')).Trim().ToArray();
            return Path.Combine(parts);
        }

        /// <summary>
        /// Gets or creates a subdirectory with the specified name.
        /// </summary>
        public static DirectoryInfo GetOrCreateSubDirectory(this DirectoryInfo @this, string subdirectoryName)
        {
            return @this.GetSubDirectory(subdirectoryName).EnsureExists();
        }

        /// <summary>
        /// Gets the subdirectory tree of this directory.
        /// </summary>
        public static IEnumerable<DirectoryInfo> GetDirectories(this DirectoryInfo @this, bool recursive)
        {
            if (!recursive) return @this.GetDirectories();
            else
            {
                var result = @this.GetDirectories().ToList();

                foreach (var sub in @this.GetDirectories())
                    result.AddRange(sub.GetDirectories(recursive: true));

                return result;
            }
        }

        /// <summary>
        /// Creates the directory if it doesn't already exist.
        /// </summary>
        public static DirectoryInfo EnsureExists(this DirectoryInfo @this)
        {
            if (!@this.Exists())
                System.IO.Directory.CreateDirectory(@this.FullName);

            // if (!folder.Exists) folder.Create(); This has caching bug in the core .NET code :-(

            return @this;
        }

        /// <summary>
        /// Determines whether this folder is empty of any files or sub-directories.
        /// </summary>
        public static bool IsEmpty(this DirectoryInfo @this)
        {
            if (@this.GetFiles().Any()) return false;
            if (@this.GetDirectories().Any()) return false;

            return true;
        }
    }
}
