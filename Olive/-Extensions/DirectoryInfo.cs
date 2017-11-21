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
        public static async Task Delete(this DirectoryInfo directory, bool recursive, bool harshly)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            if (!directory.Exists()) return;

            if (harshly && !recursive)
                throw new ArgumentException("For deleting a folder harshly, the recursive option should also be specified.");

            if (!harshly)
            {
                directory.Delete(recursive);
                return;
            }

            // Otherwise, it is harsh and recursive:
            try
            {
                // First attempt: Simple delete:
                await Task.Factory.StartNew(() => directory.Delete(recursive: true));
            }
            catch
            {
                // No loging is needed
                // Normal attempt failed. Let's try it harshly!
                await HarshDelete(directory);
            }
        }

        /// <summary>
        /// Will try to delete a specified directory by first deleting its sub-folders and files.
        /// </summary>
        static async Task HarshDelete(DirectoryInfo directory)
        {
            if (!directory.Exists()) return;

            await DoTryHard(directory, async () =>
            {
                await directory.GetFiles().Do(async (f) => await f.Delete(harshly: true));
                await directory.GetDirectories().Do(async (d) => await HarshDelete(d));
                await Task.Factory.StartNew(directory.Delete);
            }, "The system cannot delete the directory, even after several attempts. Directory: {0}");
        }

        /// <summary>
        /// Copies the entire content of a directory to a specified destination.
        /// </summary>
        public static async Task CopyTo(this DirectoryInfo source, DirectoryInfo destination, bool overwrite = false) =>
            await CopyTo(source, destination.FullName, overwrite);

        /// <summary>
        /// Determines whether the file's contents start with MZ which is the signature for EXE files.
        /// </summary>
        public static bool HasExeContent(this FileInfo file)
        {
            var twoBytes = new byte[2];
            using (var fileStream = File.Open(file.FullName, FileMode.Open))
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
        public static async Task CopyTo(this DirectoryInfo source, string destination, bool overwrite = false)
        {
            destination.AsDirectory().EnsureExists();

            foreach (var file in source.GetFiles())
                await file.CopyTo(Path.Combine(destination, file.Name).AsFile(), overwrite);

            foreach (var sub in source.GetDirectories())
                await sub.CopyTo(Path.Combine(destination, sub.Name), overwrite);
        }

        /// <summary>
        /// Copies the entire content of a directory to a specified destination.
        /// </summary>
        public static void CopyToSync(this DirectoryInfo source, string destination, bool overwrite = false)
        {
            destination.AsDirectory().EnsureExists();

            foreach (var file in source.GetFiles())
                file.CopyToSync(Path.Combine(destination, file.Name).AsFile(), overwrite);

            foreach (var sub in source.GetDirectories())
                sub.CopyToSync(Path.Combine(destination, sub.Name), overwrite);
        }

        /// <summary>
        /// Copies this file to a specified destination directiry with the original file name.
        /// </summary>
        public static async Task CopyTo(this FileInfo file, DirectoryInfo destinationDirectory, bool overwrite = false) =>
            await file.CopyTo(destinationDirectory.GetFile(file.Name), overwrite);

        public static string[] GetFiles(this DirectoryInfo folder, bool includeSubDirectories)
        {
            var result = new List<string>(folder.GetFiles().Select(f => f.FullName));

            if (includeSubDirectories)
                foreach (var subFolder in folder.GetDirectories())
                    result.AddRange(subFolder.GetFiles(includeSubDirectories: true));

            return result.ToArray();
        }

        /// <summary>
        /// Gets a file info with the specified name under this folder. That file does not have to exist already.
        /// </summary>
        public static FileInfo GetFile(this DirectoryInfo folder, string fileName) =>
            Path.Combine(folder.FullName, fileName).AsFile();

        /// <summary>
        /// Gets a subdirectory with the specified name. It does not need to exist necessarily.
        /// </summary>
        public static DirectoryInfo GetSubDirectory(this DirectoryInfo parent, string subdirectoryName)
        {
            if (subdirectoryName.IsEmpty())
                throw new ArgumentNullException("GetSubDirectory(name) expects a non-empty sub-directory name.");

            return new DirectoryInfo(Path.Combine(parent.FullName, subdirectoryName));
        }

        /// <summary>
        /// Gets or creates a subdirectory with the specified name.
        /// </summary>
        public static DirectoryInfo GetOrCreateSubDirectory(this DirectoryInfo parent, string subdirectoryName)
        {
            var result = new DirectoryInfo(Path.Combine(parent.FullName, subdirectoryName));

            result.Create();

            return result;
        }

        /// <summary>
        /// Gets the subdirectory tree of this directory.
        /// </summary>
        public static IEnumerable<DirectoryInfo> GetDirectories(this DirectoryInfo parent, bool recursive)
        {
            if (!recursive) return parent.GetDirectories();
            else
            {
                var result = parent.GetDirectories().ToList();

                foreach (var sub in parent.GetDirectories())
                    result.AddRange(sub.GetDirectories(recursive: true));

                return result;
            }
        }

        /// <summary>
        /// Creates the directory if it doesn't already exist.
        /// </summary>
        public static DirectoryInfo EnsureExists(this DirectoryInfo folder)
        {
            if (!folder.Exists())
                Directory.CreateDirectory(folder.FullName);

            // if (!folder.Exists) folder.Create(); This has caching bug in the core .NET code :-(

            return folder;
        }

        /// <summary>
        /// Determines whether this folder is empty of any files or sub-directories.
        /// </summary>
        public static bool IsEmpty(this DirectoryInfo folder)
        {
            if (folder.GetFiles().Any()) return false;
            if (folder.GetDirectories().Any()) return false;

            return true;
        }
    }
}
