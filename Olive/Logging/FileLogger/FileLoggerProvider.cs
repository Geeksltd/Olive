using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olive.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : BatchingLoggerProvider
    {
        readonly DirectoryInfo Path;
        readonly string FilePrefix;
        readonly int? MaxFileSize, MaxRetainedFiles;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options) : base(options)
        {
            Path = options.Value.LogDirectory.AsDirectory();
            FilePrefix = options.Value.FilePrefix;
            MaxFileSize = options.Value.MaxFileSize;
            MaxRetainedFiles = options.Value.RetainedFiles;
        }

        public override async Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken cancellationToken)
        {
            Path.EnsureExists(); // May not exist yet on a fresh deployment.

            // Materialised before the loop: the loop empties `messages` as it goes, and ToList makes
            // GroupBy buffer its own copy rather than read from the list while it shrinks.
            var days = messages.GroupBy(x => x.Timestamp.Date).ToList();

            foreach (var group in days)
            {
                var content = group.Select(x =>
                    x.ContextInfo.HasValue() ? x.Message + x.ContextInfo + Environment.NewLine : x.Message
                ).ToLinesString();

                await GetCurrentFile(group.Key).AppendAllTextAsync(content).ConfigureAwait(false);

                // On disk, so out of the batch: an append is not undoable, and a batch spanning midnight
                // that failed on the second day would otherwise re-append the first on every retry.
                messages.RemoveAll(x => x.Timestamp.Date == group.Key);
            }

            DeleteOldFiles();
        }

        /// <summary>
        /// The file a day's entries should be appended to: the day's file, or — once that has grown past
        /// <see cref="MaxFileSize"/> — the next roll-over file for the day (Log-yyyyMMdd-1.txt, -2.txt, …).
        /// </summary>
        FileInfo GetCurrentFile(DateTime date)
        {
            var baseName = FilePrefix + date.ToString("yyyyMMdd");
            var file = Path.GetFile(baseName + ".txt");

            if (!(MaxFileSize > 0)) return file;

            var index = 0;
            while (file.Exists() && file.Length > MaxFileSize)
                file = Path.GetFile(baseName + "-" + (++index) + ".txt");

            return file;
        }

        protected void DeleteOldFiles()
        {
            // Null means "keep everything". Called on every flush, so housekeeping must never throw.
            if (!(MaxRetainedFiles > 0)) return;

            try
            {
                // By last-write time, not name, so roll-over files (…-1.txt) retire in the right order.
                var stale = Path.GetFiles(FilePrefix + "*")
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .Skip(MaxRetainedFiles.Value);

                foreach (var file in stale)
                    try { file.Delete(); }
                    catch { /* in use or already gone: leave it for the next cycle */ }
            }
            catch
            {
                // e.g. the directory vanished between flushes. Try again next time.
            }
        }
    }
}