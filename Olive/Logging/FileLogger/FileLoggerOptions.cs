using Olive.Logging;
using System;

namespace Olive
{
    public class FileLoggerOptions : BatchingLoggerOptions
    {
        int? maxFileSize = 50 * 1024 * 1024;
        int? retainedFiles = 10;

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
        /// The maximum number of files to retaine (default: 10).
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