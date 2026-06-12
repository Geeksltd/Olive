using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Olive
{
    public enum CompressionFormat { Zip, Gzip, Tar }

    public static class CompressionIOExtensions
    {
        /// <summary>
        /// Creates a temporary compressed file from these files and returns the bytes from the generated zip file.
        /// </summary>         
        public static byte[] Compress(this IEnumerable<FileInfo> @this,
            CompressionFormat format = CompressionFormat.Zip)
        {
            if (@this.None()) return new byte[0];

            foreach (var sameName in @this.GroupBy(x => x.Name.ToUpper()))
                if (sameName.HasMany())
                    throw new Exception("Failed to compress files. Duplicate file names: " + sameName.Key);

            var tempFolder = Path.GetTempPath().AsDirectory().CreateSubdirectory(Guid.NewGuid().ToString());

            try
            {
                @this.Do(x => x.CopyTo(tempFolder.GetFile(x.Name).FullName));
                return tempFolder.Compress(format);
            }
            finally
            {
                tempFolder.DeleteIfExists();
            }
        }

        /// <summary>
        /// Creates a temporary compressed file from this directory and returns its bytes.
        /// </summary>         
        public static byte[] Compress(this DirectoryInfo @this, CompressionFormat format = CompressionFormat.Zip)
        {
            using (var zipFile = new TemporaryFile("zip"))
            {
                var file = zipFile.FilePath.AsFile();
                @this.Compress(file, format);

                return file.ReadAllBytes();
            }
        }

        /// <summary>
        /// Creates a compressed file from this directory.
        /// </summary>        
        /// <param name="destination">Path of the target zip file to generate.</param>
        public static void Compress(this DirectoryInfo @this, FileInfo destination,
            CompressionFormat format = CompressionFormat.Zip, bool overwrite = false)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            if (destination.Exists() && !overwrite)
                throw new Exception("Destination file already exists: " + destination.FullName);

            ArchiveType archiveType;
            WriterOptions options;

            switch (format)
            {
                case CompressionFormat.Zip:
                    archiveType = ArchiveType.Zip;
                    options = new WriterOptions(CompressionType.Deflate);
                    break;
                case CompressionFormat.Gzip:
                    archiveType = ArchiveType.GZip;
                    options = WriterOptions.ForGZip();
                    break;
                case CompressionFormat.Tar:
                    archiveType = ArchiveType.Tar;
                    options = new WriterOptions(CompressionType.None);
                    break;
                default: throw new NotSupportedException();
            }

            using (var stream = File.OpenWrite(destination.FullName))
            using (var writer = WriterFactory.OpenWriter(stream, archiveType, options))
                foreach (var file in @this.GetFiles("*", SearchOption.AllDirectories))
                {
                    var key = file.FullName.Substring(@this.FullName.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    using (var source = file.OpenRead())
                        writer.Write(key, source, file.LastWriteTime);
                }
        }

        /// <summary>
        /// Decompresses this file into a specified directory.
        /// </summary>
        /// <param name="destination">Desired decompression desination path.</param>
        public static void Decompress(this FileInfo @this, DirectoryInfo destination,
            bool extractFullPath = true, bool overwrite = false)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            @this.ExistsOrThrow();

            if (destination.Exists() && !overwrite)
                throw new Exception("Destination file already exists: " + destination.FullName);

            var options = new ExtractionOptions
            {
                ExtractFullPath = extractFullPath,
                Overwrite = overwrite
            };

            using (var stream = File.OpenRead(@this.FullName))
            using (var reader = ReaderFactory.OpenReader(stream, new ReaderOptions()))
                while (reader.MoveToNextEntry())
                    if (!reader.Entry.IsDirectory)
                        reader.WriteEntryToDirectory(destination.FullName, options);
        }
    }
}