using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
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
        /// Creates a log-string from the Exception.
        /// The result includes the stacktrace, innerexception et cetera, separated by Environment.NewLine.
        /// </summary>
        /// <param name="this">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        public static string ToLogString(this Exception @this, string additionalMessage)
        {
            var r = new StringBuilder();
            r.AppendLine(@this.ToFullMessage(additionalMessage, includeStackTrace: true, includeData: true));
            return r.ToString();
        }

        public static string ToLogString(this Exception ex) => ToLogString(ex, null);

        /// <summary>
        /// Adds a piece of data to this exception.
        /// </summary>
        public static Exception AddData(this Exception exception, string key, object value)
        {
            if (value != null)
            {
                try
                {
                    exception.Data.Add(key, value);
                }
                catch
                {
                    // Not serializable
                    try
                    {
                        exception.Data.Add(key, value.ToString());
                    }
                    catch
                    {
                        // No logging is needed
                    }
                }
            }

            return exception;
        }

        public static async Task<string> GetResponseBody(this WebException ex)
        {
            if (ex.Response == null) return null;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Returns a more complete text dump of this exception, than just its text.
        /// </summary>
        public static string ToFullMessage(this Exception @this, string additionalMessage, bool includeStackTrace, bool includeData)
        {
            var err = @this ?? throw new NullReferenceException("This exception object is null");

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLineIf(additionalMessage, additionalMessage.HasValue());

            while (err != null)
            {
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

                if (err is TargetInvocationException)
                    err = err.InnerException;

                err = err.InnerException;
                if (err != null)
                {
                    resultBuilder.AppendLine();
                    if (includeStackTrace)
                        resultBuilder.AppendLine("###############################################");
                    resultBuilder.Append("Base issue: ");
                }
            }

            if (includeStackTrace && @this.StackTrace.HasValue())
            {
                var stackLines = @this.StackTrace.Or("").Trim().ToLines();
                stackLines = stackLines.Except(l => l.Trim().StartsWith("at System.Data.")).ToArray();
                resultBuilder.AppendLine(stackLines.ToString("\r\n\r\n").WithPrefix("\r\n--------------------------------------\r\nSTACK TRACE:\r\n\r\n"));
            }

            return resultBuilder.ToString();
        }
    }
}