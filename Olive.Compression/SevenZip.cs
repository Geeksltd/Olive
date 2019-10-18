using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Olive.Compression
{
    public class SevenZip
    {
        public static string SEVEN_ZIP_EXE_FILE_PATH; // C:\Program Files\7-Zip\7z.exe

        static SevenZip()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                SEVEN_ZIP_EXE_FILE_PATH = GetSevenZipFileAddress();
            else
                SEVEN_ZIP_EXE_FILE_PATH = "7z";
        }

        public enum CompressionMode
        {
            Fastest,
            Fast,
            Normal,
            HighCompression,
            MaximumCompression
        }

        [EscapeGCop("This paths will not change.")]
        static string GetSevenZipFileAddress()
        {
            var programFilesOptions = new List<string>
                {
                    @"C:\Program Files\",
                    @"C:\Program Files(x86)\",
                    @"D:\Program Files\",
                    @"D:\Program Files(x86)\",
                    @"E:\Program Files\",
                    @"E:\Program Files(x86)\",
                    @"F:\Program Files\",
                    @"F:\Program Files(x86)\"
                };

            try { programFilesOptions.Add(Environment.GetEnvironmentVariable("ProgramFiles(x86)")); }
            catch { /* No Logging Needed */ }

            try { programFilesOptions.Add(Environment.GetEnvironmentVariable("ProgramFiles")); }
            catch { /* No Logging Needed */ }

            foreach (var item in programFilesOptions.Distinct())
            {
                try
                {
                    var file = item.AsDirectory().GetFile( "7-Zip\\7z.exe");
                    if ( file.Exists()) return file.FullName;
                }
                catch { }
            }

            throw new Exception("7Zip was not found in any of: \r\n" + programFilesOptions.ToLinesString());
        }

        static string GetCompressionModeSwitch(CompressionMode mode)
        {
            switch (mode)
            {
                case CompressionMode.Fastest: return "-mx1";
                case CompressionMode.Fast: return "-mx3";
                case CompressionMode.Normal: return "-mx5";
                case CompressionMode.HighCompression: return "-mx7";
                case CompressionMode.MaximumCompression: return "-mx9";
                default: throw new NotSupportedException(mode + " is not supported in GetCompressionModeSwitch()");
            }
        }

        /// <summary>
        /// Compresses the specified folders into a 7 Zip archive folder.
        /// </summary>
        public static Process Compress(string zipFileName, params string[] foldersToCompress) =>
            Compress(zipFileName, null, foldersToCompress);

        /// <summary>
        /// Compresses the specified folders into a 7 Zip archive folder.
        /// </summary>
        /// <param name="splitSize">The maximum size of each splitted size in Kilo Bytes</param>        
        public static Process Compress(string zipFileName, int? splitSize, params string[] foldersToCompress) =>
            Compress(zipFileName, splitSize, CompressionMode.Normal, foldersToCompress);

        /// <summary>
        /// Compresses the specified folders into a 7 Zip archive folder.
        /// </summary>
        /// <param name="splitSize">The maximum size of each splitted size in Kilo Bytes</param>        
        public static Process Compress(string zipFileName, int? splitSize, CompressionMode compressionMode, params string[] foldersToCompress) =>
            Compress(zipFileName, splitSize, compressionMode, null, foldersToCompress, new string[0]);

        /// <summary>
        /// Compresses the specified source files into a temp 7Zip file and returns the temp 7Zip file.
        /// </summary>
        public static async Task<FileInfo> Compress(IEnumerable<FileInfo> sourceFiles, CompressionMode compressionMode = CompressionMode.Normal, string customParameters = null)
        {
            var result = Path.GetTempPath().AsDirectory().GetFile("Temp7Zip-" + Guid.NewGuid() + ".7z");
            await Compress(result, sourceFiles, compressionMode, customParameters);

            return result;
        }

        /// <summary>
        /// Compresses the specified source files into a 7Zip file and returns the data of the 7Zip file. The temp file is deleted.
        /// </summary>
        public static async Task<byte[]> CompressToBytes(IEnumerable<FileInfo> sourceFiles, CompressionMode compressionMode = CompressionMode.Normal, string customParameters = null)
        {
            var tempFolder = Path.GetTempPath().AsDirectory().GetOrCreateSubDirectory("Temp7Zip-" + Guid.NewGuid());
            var tempFile = tempFolder.GetFile("Files.7z");

            try
            {
                await Compress(tempFile, sourceFiles, compressionMode, customParameters);
                return await tempFile.ReadAllBytesAsync();
            }
            finally
            {
                await tempFolder?.DeleteAsync(recursive: true, harshly: true);
            }
        }

        /// <summary>
        /// Creates a 7Zip file from the specified files.
        /// </summary>
        public static async Task Compress(FileInfo destinationPath, IEnumerable<FileInfo> sourceFiles, CompressionMode compressionMode = CompressionMode.Normal, string customParameters = null)
        {
            var files = sourceFiles.ToArray();
            if (files.Distinct(x => x.Name.ToLower()).Count() != files.Count()) throw new Exception("File names must be unique.");

            var tempFolder = Path.GetTempPath().AsDirectory().GetOrCreateSubDirectory("7ZipTemp." + Guid.NewGuid());
            var filesFolder = tempFolder.GetOrCreateSubDirectory(Path.GetFileNameWithoutExtension(destinationPath.FullName));
            try
            {
                await Task.WhenAll(files.Select(f => f.CopyTo(filesFolder)));

                Compress(destinationPath.FullName, default(int?), compressionMode, customParameters, new[] { filesFolder.FullName }).WaitForExit();
            }
            finally
            {
                await tempFolder.DeleteAsync(recursive: true, harshly: true);
            }
        }

        /// <summary>
        /// Compresses the specified folders into a 7 Zip archive folder.
        /// </summary>
        /// <param name="excludedFilePatterns">Use wildcards. Example: *\Folder\Sub-folder\*</param>
        public static Process Compress(string zipFileName, int? splitSize, CompressionMode compressionMode, string customParameters, string[] foldersToCompress, string[] excludedFilePatterns = null)
        {
            if (zipFileName.IsEmpty())
                throw new ArgumentNullException(nameof(zipFileName));

            if (foldersToCompress == null || foldersToCompress.Length == 0)
                throw new ArgumentNullException(nameof(foldersToCompress));

            if (splitSize < 1)
                throw new ArgumentException("The minimum possible split size for 7 Zipper is 1KB.");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !File.Exists(SEVEN_ZIP_EXE_FILE_PATH))
                throw new Exception("7 Zip is not installed. Could not find the file at " + SEVEN_ZIP_EXE_FILE_PATH);

            var command = new ProcessStartInfo(SEVEN_ZIP_EXE_FILE_PATH, "a " + GetCompressionModeSwitch(compressionMode));

            if (splitSize.HasValue)
                command.Arguments += " -v\"" + splitSize + "k\" ";

            if (customParameters.HasValue())
                command.Arguments += " " + customParameters + " ";

            if (excludedFilePatterns.HasAny())
                foreach (var ex in excludedFilePatterns)
                    command.Arguments += " -xr!\"" + ex + "\" ";

            // Add destination folder:
            command.Arguments += " \"" + zipFileName + "\" ";

            // Add source folders:
            command.Arguments += foldersToCompress.Select(pt => " \"" + pt + "\"").ToString(" ");

            return Process.Start(command);
        }
    }
}
