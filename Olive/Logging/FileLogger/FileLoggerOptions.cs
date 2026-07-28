using System;
using Olive.Logging;

namespace Olive
{
    public class FileLoggerOptions : BatchingLoggerOptions
    {
        int? maxFileSize = 50 * 1024 * 1024;
        int? retainedFiles;

        /// <summary>
        /// Gets or sets the filename prefix to use for log files.
        /// Defaults to <c>Log-</c>.
        /// </summary>
        public string FilePrefix { get; set; } = "Log-";

        /// <summary>
        /// The directory in which log files will be written, relative to the app process.
        /// Default to <c>Logs</c>
        /// </summary> 
        public string LogDirectory { get; set; } = "Logs";

        /// <summary>
        /// Gets or sets a positive value representing the maximum log size in bytes or null for no limit.
        /// Once the log is full, no more messages will be appended.
        /// Defaults to <c>50MB</c>.
        /// </summary>
        public int? MaxFileSize
        {
            get => maxFileSize;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                maxFileSize = value;
            }
        }

        /// <summary>
        /// The maximum number of log files to keep, or null — the default — to keep every one. Null by
        /// default because deletion is destructive, so opting into it is the caller's decision.
        /// <para>
        /// This counts FILES, not days: with <see cref="MaxFileSize"/> set, one busy day produces as
        /// many as it needs (Log-yyyyMMdd.txt, Log-yyyyMMdd-1.txt, …) and each counts against the cap.
        /// </para>
        /// </summary>
        public int? RetainedFiles
        {
            get => retainedFiles;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
                retainedFiles = value;
            }
        }
    }
}