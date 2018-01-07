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

        public static string NameWithoutExtension(this FileInfo file) => Path.GetFileNameWithoutExtension(file.FullName);

        /// <summary>
        /// Determines whether or not this directory exists.
        /// Note: The standard Exists property has a caching bug, so use this for accurate result.
        /// </summary>
        public static bool Exists(this DirectoryInfo folder)
        {
            if (folder == null) return false;
            return Directory.Exists(folder.FullName);
        }

        /// <summary>
        /// Determines whether or not this file exists. 
        /// Note: The standard Exists property has a caching bug, so use this for accurate result.
        /// </summary>
        public static bool Exists(this FileInfo file)
        {
            if (file == null) return false;
            return File.Exists(file.FullName);
        }

        /// <summary>
        /// Gets the total size of all files in this directory.
        /// </summary>
        public static long GetSize(this DirectoryInfo folder, bool includeSubDirectories = true)
            => folder.GetFiles(includeSubDirectories).Sum(x => x.AsFile().Length);

        /// <summary>
        /// Gets the size of this folder in human readable text.
        /// </summary>
        public static string GetSizeText(this DirectoryInfo folder, bool includeSubDirectories = true, int round = 1) =>
            folder.GetSize(includeSubDirectories).ToFileSizeString(round);

        /// <summary>
        /// Gets the size of this file in human readable text.
        /// </summary>
        public static string GetSizeText(this FileInfo file, int round = 1) => file.Length.ToFileSizeString(round);

        /// <summary>
        /// Detects the characters which are not acceptable in File System and replaces them with a hyphen.
        /// </summary>
        /// <param name="replacement">The character with which to replace invalid characters in the name.</param>
        public static string ToSafeFileName(this string name, char replacement = '-')
        {
            if (name.IsEmpty()) return string.Empty;

            var controlCharacters = name.Where(c => char.IsControl(c));

            var invalidChars = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }.Concat(controlCharacters);

            foreach (var c in invalidChars)
                name = name.Replace(c, replacement);

            if (replacement.ToString().HasValue())
                name = name.KeepReplacing(replacement.ToString() + replacement, replacement.ToString());

            return name.Summarize(255).TrimEnd("...");
        }

        /// <summary>
        /// Executes this EXE file and returns the standard output.
        /// </summary>
        public static string Execute(this FileInfo exeFile, string args, bool waitForExit = true) =>
            Execute(exeFile, args, waitForExit, null);

        /// <summary>
        /// Executes this EXE file and returns the standard output.
        /// </summary>
        public static string Execute(this FileInfo exeFile, string args, bool waitForExit, Action<Process> configuration)
        {
            var output = new StringBuilder();

            var process = new Process
            {
                EnableRaisingEvents = true,

                StartInfo = new ProcessStartInfo
                {
                    FileName = exeFile.FullName,
                    Arguments = args,
                    WorkingDirectory = exeFile.Directory.FullName,
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
                if (e.Data.HasValue()) output.AppendLine(e.Data);
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) output.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Error running '{exeFile.FullName}':{output}");
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the mime type based on the file extension.
        /// </summary>
        public static string GetMimeType(this FileInfo file)
        {
            switch (file.Extension.OrEmpty().TrimStart("."))
            {
                case "doc": case "docx": return "application/msword";
                case "pdf": return "application/pdf";
                case "ppt": return "application/powerpoint";
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
        public static IEnumerable<FileInfo> GetFilesOrEmpty(this DirectoryInfo folder, string searchPattern)
        {
            if (folder == null || !folder.Exists())
                return Enumerable.Empty<FileInfo>();

            return folder.GetFiles(searchPattern);
        }
    }
}