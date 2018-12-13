using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olive.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive
{
    [ProviderAlias("File")]
    class FileLoggerProvider : BatchingLoggerProvider
    {
        DirectoryInfo Path;
        readonly string FilePrefix;
        readonly int? MaxFileSize, MaxRetainedFiles;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options) : base(options)
        {
            Path = options.Value.LogDirectory.AsDirectory();
            FilePrefix = options.Value.FilePrefix;
            MaxFileSize = options.Value.MaxFileSize;
            MaxRetainedFiles = options.Value.RetainedFiles;
        }

        public override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
        {
            foreach (var group in messages.GroupBy(x => x.Timestamp.Date))
            {
                var file = Path.GetFile(FilePrefix + group.Key.ToString("yyyyMMdd") + ".txt");
                if (MaxFileSize > 0 && file.Exists() && file.Length > MaxFileSize) return;

                await file.AppendAllTextAsync(group.Select(x => x.Message).ToLinesString());
            }
        }

        protected void DeleteOldFiles()
        {
            if (MaxRetainedFiles <= 0) return;

            Path.GetFiles(FilePrefix + "*").OrderByDescending(f => f.Name)
                .Skip(MaxRetainedFiles.Value).Do(x => x.Delete());
        }
    }
}