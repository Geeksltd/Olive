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

        public static async Task<string> GetResponseBody(this WebException @this)
        {
            if (@this.Response == null) return default;

            using var reader = new StreamReader(@this.Response.GetResponseStream());
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Returns a more complete text dump of this exception, than just its text.
        /// </summary>
        public static string ToFullMessage(this Exception @this, string additionalMessage, bool includeStackTrace, bool includeData)
        {
            var err = @this ?? throw new NullReferenceException("This exception object is null");

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLineIf(additionalMessage);

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
                stackLines = stackLines.Except(l => l.Trim().StartsWith("at System.Data.")).ToArray();
                resultBuilder.AppendLine(stackLines.ToString("\r\n\r\n").WithPrefix("\r\n--------------------------------------\r\nSTACK TRACE:\r\n\r\n"));
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

        public static bool IsNetworkRelated(this Exception ex)
        {
            if (ex.Message.StartsWith("The request was aborted")) return true;
            if (ex.Message.StartsWith("The network connection was lost")) return true;
            if (ex.Message.StartsWith("A task was canceled.")) return true;
            if (ex.Message.StartsWith("A server with the specified hostname could not be found.")) return true;
            if (ex.Message.StartsWith("An SSL error has occurred and a secure connection to the server cannot be made.")) return true;

            if (ex.Message.Contains("attempts to download a URL all failed")) return true;

            var fullMessage = ex.ToFullMessage();
            if (fullMessage.Contains("Unable to read data from the transport connection")) return true;
            if (fullMessage.Contains("ConnectFailure (The requested address is not valid in this context)")) return true;
            if (fullMessage.Contains("ConnectFailure (Connection timed out)")) return true;
            if (fullMessage.Contains("ConnectFailure (Network is unreachable)")) return true;
            if (fullMessage.Contains("ConnectFailure (Connection refused)")) return true;
            if (fullMessage.Contains("ConnectFailure (No route to host)")) return true;
            if (fullMessage.Contains("ConnectFailure (Network subsystem is down)")) return true;
            if (fullMessage.Contains("NameResolutionFailure")) return true;
            if (fullMessage.Contains("SecureChannelFailure")) return true;
            if (fullMessage.Contains("Error getting response stream (ReadDoneAsync2): ReceiveFailure")) return true;

            if (fullMessage.ContainsAny(new[] { "Zebble.Device.Network+<Download>", "System.Net.Sockets.NetworkStream" }))
            {
                if (fullMessage.Contains("The operation has timed out")) return true;
                if (fullMessage.Contains("The operation was canceled.")) return true;
            }

            return false;
        }
    }
}