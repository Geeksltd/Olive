using System;
using System.Text;

namespace Olive
{
    partial class OliveExtensions
    {
        [System.Diagnostics.DebuggerStepThrough]
        public static void AddFormattedLine(this StringBuilder builder, string format, params object[] args)
        {
            try
            {
                builder.AppendFormat(format, args);
                builder.AppendLine();
            }
            catch (Exception ex)
            {
                throw new FormatException("Could not add formatted line of \"" +
                    format + "\" with the following parameters: {" +
                    args.ToString(", ") + "}.", ex);
            }
        }

        /// <summary>
        /// Wraps the content of this string builder with the provided text blocks.
        /// </summary>
        public static void WrapIn(this StringBuilder builder, string left, string right)
        {
            builder.Insert(0, left);
            builder.Append(right);
        }

        /// <summary>
        /// Wraps the content of this string builder with the provided lines of text.
        /// A line-break will be added to the left element, and another line break will be added before the right element.
        /// </summary>
        public static void WrapInLines(this StringBuilder builder, string left, string right)
        {
            builder.Insert(0, left + Environment.NewLine);
            builder.Append(Environment.NewLine + right);
        }

        public static void AppendIf(this StringBuilder builder, string text, bool condition)
        {
            if (condition) builder.Append(text);
        }

        public static void AppendLineIf(this StringBuilder builder, string text, bool condition)
        {
            if (condition) builder.AppendLine(text);
        }

        public static void AppendLineIf(this StringBuilder builder, string text)
        {
            if (text.HasValue()) builder.AppendLine(text);
        }
    }
}
