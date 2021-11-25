using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Olive
{
    partial class OliveExtensions
    {
        static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public static string NameWithoutExtension(this FileInfo @this) => Path.GetFileNameWithoutExtension(@this.FullName);

        /// <summary>
        /// Determines whether or not this directory exists.
        /// Note: The standard Exists property has a caching bug, so use this for accurate result.
        /// </summary>
        public static bool Exists(this DirectoryInfo @this)
        {
            if (@this == null) return false;
            return System.IO.Directory.Exists(@this.FullName);
        }

        /// <summary>
        /// Determines whether or not this file exists. 
        /// Note: The standard Exists property has a caching bug, so use this for accurate result.
        /// </summary>
        public static bool Exists(this FileInfo @this)
        {
            if (@this == null) return false;
            return File.Exists(@this.FullName);
        }

        /// <summary>
        /// Gets the total size of all files in this directory.
        /// </summary>
        public static long GetSize(this DirectoryInfo @this, bool includeSubDirectories = true)
            => @this.GetFiles(includeSubDirectories).Sum(x => x.AsFile().Length);

        /// <summary>
        /// Gets the size of this folder in human readable text.
        /// </summary>
        public static string GetSizeText(this DirectoryInfo @this, bool includeSubDirectories = true, int round = 1) =>
            @this.GetSize(includeSubDirectories).ToFileSizeString(round);

        /// <summary>
        /// Gets the size of this file in human readable text.
        /// </summary>
        public static string GetSizeText(this FileInfo @this, int round = 1) => @this.Length.ToFileSizeString(round);

        /// <summary>
        /// Detects the characters which are not acceptable in File System and replaces them with a hyphen.
        /// </summary>
        /// <param name="replacement">The character with which to replace invalid characters in the name.</param>
        public static string ToSafeFileName(this string @this, char replacement = '-')
        {
            if (@this.IsEmpty()) return string.Empty;

            var controlCharacters = @this.Where(c => char.IsControl(c));

            var invalidChars = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }.Concat(controlCharacters);

            foreach (var c in invalidChars)
                @this = @this.Replace(c, replacement);

            if (replacement.ToString().HasValue())
                @this = @this.KeepReplacing(replacement.ToString() + replacement, replacement.ToString());

            return @this.Summarize(255).TrimEnd("...");
        }

        /// <summary>
        /// Executes this EXE file and returns the standard output.
        /// </summary>
        public static string Execute(this FileInfo @this, string args, bool waitForExit = true, Action<Process> configuration = null)
        {
            var output = new StringBuilder();

            var process = new Process
            {
                EnableRaisingEvents = true,

                StartInfo = new ProcessStartInfo
                {
                    FileName = @this.FullName,
                    Arguments = args,
                    WorkingDirectory = @this.Directory.FullName,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            configuration?.Invoke(process);

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data.HasValue()) lock (output) output.AppendLine(e.Data);
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) lock (output) output.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new($"Error running '{@this.FullName}':{output}");
                else process.Dispose();
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public static string GetMimeType(this FileInfo @this)
        {
            return @this.Extension.ToLower().OrEmpty().TrimStart(".") switch
            {
                "doc" => "application/msword",
                "ppt" => "application/vnd.ms-powerpoint",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "pdf" => "application/pdf",
                "rtf" => "application/rtf",
                "gz" => "application/x-gzip",
                "zip" => "application/zip",
                "mpga" or "mp2" => "audio/mpeg",
                "ram" => "audio/x-pn-realaudio",
                "ra" => "audio/x-realaudio",
                "wav" => "audio/x-wav",
                "gif" => "image/gif",
                "jpeg" or "jpg" or "jpe" => "image/jpeg",
                "png" => "image/png",
                "tiff" or "tif" => "image/tiff",
                "svg" => "image/svg+xml",
                "html" or "htm" => "text/html",
                "txt" => "text/plain",
                "mpeg" or "mpg" or "mpe" => "video/mpeg",
                "mov" or "qt" => "video/quicktime",
                "avi" => "video/avi",
                "mid" => "audio/mid",
                "midi" => "application/x-midi",
                "divx" => "video/divx",
                "webm" => "video/webm",
                "wma" => "audio/x-ms-wma",
                "mp3" => "audio/mp3",
                "ogg" => "audio/ogg",
                "rma" => "audio/rma",
                "mp4" => "video/mp4",
                "wmv" => "video/x-ms-wmv",
                "f4v" => "video/x-f4v",
                "ogv" => "video/ogg",
                "3gp" => "video/3gpp",
                _ => "application/octet-stream",
            };
        }

        /// <summary>
        /// Gets the files in this folder. If this folder is null or non-existent it will return an empty array.
        /// </summary>
        public static IEnumerable<FileInfo> GetFilesOrEmpty(this DirectoryInfo @this, string searchPattern)
        {
            if (@this == null || !@this.Exists())
                return Enumerable.Empty<FileInfo>();

            return @this.GetFiles(searchPattern);
        }

        /// <summary>
        /// Gets this file's original exact file name with the correct casing.
        /// </summary>
        public static string GetExactFullName(this FileSystemInfo @this)
        {
            var path = @this.FullName;
            if (!File.Exists(path) && !System.IO.Directory.Exists(path)) return path;

            var asDirectory = new DirectoryInfo(path);
            var parent = asDirectory.Parent;

            if (parent == null) // Drive:
                return asDirectory.Name.ToUpper();

            return Path.Combine(parent.GetExactFullName(), parent.GetFileSystemInfos(asDirectory.Name)[0].Name);
        }

        /// <summary>
        /// If this file exists, it will simply return it. 
        /// Otherwise it will throw a FileNotFoundException with the message of 'File not found: {path}'.
        /// </summary>
        public static FileInfo ExistsOrThrow(this FileInfo file)
        {
            if (!file.Exists())
                throw new FileNotFoundException("File not found: " + file.FullName);

            return file;
        }

        /// <summary>
        /// If this directory exists, it will simply return it. 
        /// Otherwise it will throw a DirectoryNotFoundException with the message of 'Directory not found: {path}'.
        /// </summary>
        public static DirectoryInfo ExistsOrThrow(this DirectoryInfo directory)
        {
            if (!directory.Exists())
                throw new DirectoryNotFoundException("Directory not found: " + directory.FullName);

            return directory;
        }

        /// <summary>
        /// Creates the file if it doesn't already exist.
        /// </summary>
        public static FileInfo EnsureExists(this FileInfo @this)
        {
            if (!@this.Exists())
                File.Create(@this.FullName).Dispose();

            return @this;
        }
    }
}