using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MSharp.Build
{
    static class Extensions
    {
        /// <summary>
        /// Determines whether this instance of string is not null or empty.
        /// </summary>
        public static bool HasValue(this string text) => !string.IsNullOrEmpty(text);

        /// <summary>
        /// Returns this text with the specified prefix if this has a value. If this text is empty or null, it will return empty string.
        /// </summary>
        public static string WithPrefix(this string @this, string prefix)
        {
            if (@this.IsEmpty()) return string.Empty;
            else return prefix + @this;
        }

        /// <summary>
        /// Gets the lines of this string.
        /// </summary>
        public static string[] ToLines(this string @this)
        {
            if (@this is null) return new string[0];

            return @this.Split('\n').Select(l => l.Trim('\r')).ToArray();
        }

        /// <summary>
        /// Determines whether this instance of string is null or empty.
        /// </summary>
        public static bool IsEmpty(this string text) => string.IsNullOrEmpty(text);

        /// <summary>
        /// If this string object is null, it will return empty string. Otherwise it will trim the text and return it.
        /// </summary>
        public static string TrimOrEmpty(this string @this) => @this?.Trim() ?? string.Empty;

        /// <summary>
        /// Returns a more complete text dump of this exception, than just its text.
        /// </summary>
        public static string ToFullMessage(this Exception @this, string additionalMessage, bool includeStackTrace, bool includeData)
        {
            var err = @this ?? throw new NullReferenceException("This exception object is null");

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine(additionalMessage);

            while (err != null)
            {
                if (err is TargetInvocationException || err is AggregateException)
                {
                    err = err.InnerException;
                    continue;
                }

                resultBuilder.AppendLine(err.Message);

                if (includeData && err.Data != null && err.Data.Count > 0)
                {
                    resultBuilder.AppendLine("\r\nException Data:\r\n{");
                    foreach (var i in err.Data)
                        resultBuilder.AppendLine(ToLogText(i).WithPrefix("    "));
                    resultBuilder.AppendLine("}");
                }

                if (err is ReflectionTypeLoadException)
                {
                    foreach (var loaderEx in (err as ReflectionTypeLoadException).LoaderExceptions)
                        resultBuilder.AppendLine("Type load exception: " + loaderEx.ToFullMessage());
                }

                err = err.InnerException;
                if (err != null)
                {
                    resultBuilder.AppendLine();
                    if (includeStackTrace)
                        resultBuilder.AppendLine("###############################################");
                    resultBuilder.Append("Base issue: ");
                }
            }

            var stack = @this.GetUsefulStack().TrimOrEmpty();
            if (includeStackTrace && stack.HasValue())
            {
                var stackLines = stack.ToLines();
                stackLines = stackLines.Where
                    (l => !l.Trim().StartsWith("at System.Data.")).ToArray();
                resultBuilder.AppendLine(string.Join("\r\n\r\n", stackLines).WithPrefix("\r\n--------------------------------------\r\nSTACK TRACE:\r\n\r\n"));
            }

            return resultBuilder.ToString();
        }

        public static string GetUsefulStack(this Exception @this)
        {
            if (@this.InnerException == null) return @this.StackTrace;
            if (@this is TargetInvocationException || @this is AggregateException)
                return @this.InnerException.GetUsefulStack();
            return @this.StackTrace;
        }

        public static string ToFullMessage(this Exception ex)
        {
            return ToFullMessage(ex, additionalMessage: null, includeStackTrace: false, includeData: false);
        }

        /// <summary>
        /// It returns ToString for all objects except DictionaryEntries.
        /// </summary>
        public static string ToLogText(object item)
        {
            try
            {
                if (item is DictionaryEntry d) return d.Key + ": " + d.Value;
                return item.ToString();
            }
            catch
            {
                // No logging is needed
                return "?";
            }
        }

        /// <summary>
        /// Converts this path into a file object.
        /// </summary> 
        public static FileInfo AsFile(this string @this)
        {
            if (@this.IsEmpty()) return null;
            return new FileInfo(@this);
        }

        /// <summary>
        /// Executes this EXE file and returns the standard output.
        /// </summary>
        public static string Execute(this FileInfo @this, string args, bool waitForExit = true, Action<Process> configuration = null)
        {
            if (!File.Exists(@this.FullName))
                throw new Exception("!!! File not found: " + @this.FullName);

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

    }
}
