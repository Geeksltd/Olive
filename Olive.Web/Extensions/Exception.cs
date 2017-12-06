namespace Olive.Web
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Returns a more complete text dump of this exception, than just its text.
        /// </summary>
        public static string ToFullMessage(this Exception error, string additionalMessage, bool includeStackTrace, bool includeData)
        {
            if (error == null)
                throw new NullReferenceException("This exception object is null");
            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLineIf(additionalMessage, additionalMessage.HasValue());
            var err = error;
            while (err != null)
            {
                resultBuilder.AppendLine(err.Message);
                if (includeData && err.Data != null)
                {
                    resultBuilder.AppendLine("\r\nException Data:\r\n{");
                    foreach (var i in err.Data)
                        resultBuilder.AppendLine(Olive.OliveExtensions.ToLogText(i).WithPrefix("    "));

                    resultBuilder.AppendLine("}");
                }

                if (err is ReflectionTypeLoadException)
                {
                    foreach (var loaderEx in (err as ReflectionTypeLoadException).LoaderExceptions)
                        resultBuilder.AppendLine("Type load exception: " + loaderEx.ToFullMessage());
                }

                // try
                // {
                //    resultBuilder.AppendLineIf((err as HttpUnhandledException)?.GetHtmlErrorMessage().TrimBefore("Server Error"));
                // }
                // catch
                // {
                //    // No logging is needed
                // }

                err = err.InnerException;
                if (err != null)
                {
                    resultBuilder.AppendLine();
                    if (includeStackTrace)
                        resultBuilder.AppendLine("###############################################");
                    resultBuilder.Append("Base issue: ");
                }
            }

            if (includeStackTrace && error.StackTrace.HasValue())
            {
                var stackLines = error.StackTrace.Or("").Trim().ToLines();
                stackLines = stackLines.Except(l => l.Trim().StartsWith("at System.Data.")).ToArray();
                resultBuilder.AppendLine(stackLines.ToString("\r\n\r\n").WithPrefix("\r\n--------------------------------------\r\nSTACK TRACE:\r\n\r\n"));
            }

            return resultBuilder.ToString();
        }

        public static async Task<string> GetResponseBody(this WebException ex)
        {
            if (ex.Response == null) return null;

            using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                return await reader.ReadToEndAsync();
        }
    }
}
