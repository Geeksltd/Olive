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
                    throw new Exception($"Error running '{@this.FullName}':{output}");
                else process.Dispose();
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public static string GetMimeType(this FileInfo @this)
        {
            switch (@this.Extension.ToLower().OrEmpty().TrimStart("."))
            {
                case "doc": case "docx": return "application/msword";
                case "pdf": return "application/pdf";
                case "rtf": return "application/rtf";
                case "gz": return "application/x-gzip";
                case "zip": return "application/zip";
                case "mpga": case "mp2": return "audio/mpeg";
                case "ram": return "audio/x-pn-realaudio";
                case "ra": return "audio/x-realaudio";
                case "wav": return "audio/x-wav";
                case "gif": return "image/gif";
                case "jpeg": case "jpg": case "jpe": return "image/jpeg";
                case "png": return "image/png";
                case "tiff": case "tif": return "image/tiff";
                case "svg": return "image/svg+xml";
                case "html": case "htm": return "text/html";
                case "txt": return "text/plain";
                case "mpeg": case "mpg": case "mpe": return "video/mpeg";
                case "mov": case "qt": return "video/quicktime";
                case "avi": return "video/avi";
                case "mid": return "audio/mid";
                case "midi": return "application/x-midi";
                case "divx": return "video/divx";
                case "webm": return "video/webm";
                case "wma": return "audio/x-ms-wma";
                case "mp3": return "audio/mp3";
                case "ogg": return "audio/ogg";
                case "rma": return "audio/rma";
                case "mp4": return "video/mp4";
                case "wmv": return "video/x-ms-wmv";
                case "f4v": return "video/x-f4v";
                case "ogv": return "video/ogg";
                case "3gp": return "video/3gpp";
                default: return "application/octet-stream";
            }
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