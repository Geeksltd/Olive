using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Olive.Compression
{
    public static class SharpCompress
    {
        /// <summary>
        /// A fluent API for compressing archives
        /// </summary>
        /// <param name="format">Format of compressed file</param>
        /// <param name="folderPath">The folder path that you want to compress</param>
        /// <param name="destinationPath">Desired folder that your compressed file is going to be saved in</param>
        /// <param name="filename">Filename including extension name</param>
        public static void Compress(CompressionFormats format, string folderPath, string destinationPath, string filename)
        {
            if (format == CompressionFormats.zip)
            {
                using (var archive = ZipArchive.Create())
                {
                    archive.AddAllFromDirectory(folderPath);
                    archive.SaveTo(Path.Combine(destinationPath, filename), CompressionType.Deflate);
                }
            }

            else if (format == CompressionFormats.tar)
            {
                using (var archive = TarArchive.Create())
                {
                    archive.AddAllFromDirectory(folderPath);
                    archive.SaveTo(Path.Combine(destinationPath, filename), CompressionType.None);
                }
            }

            else if (format == CompressionFormats.gzip)
            {
                using (var archive = GZipArchive.Create())
                {
                    archive.AddAllFromDirectory(folderPath);
                    archive.SaveTo(Path.Combine(destinationPath, filename), CompressionType.Deflate);
                }
            }

            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// This method used for decompressing archives
        /// </summary>
        /// <param name="filePath">The compressed file desired to decompress</param>
        /// <param name="destinationPath">Desired decompression desination path</param>
        /// <param name="extractFullPath"></param>
        /// <param name="overrite"></param>
        public static void Decompress(string filePath, string destinationPath, bool extractFullPath = true, bool overrite = true)
        {
            using (Stream stream = File.OpenRead(filePath))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Console.WriteLine(reader.Entry.Key);
                        reader.WriteEntryToDirectory(destinationPath, new ExtractionOptions()
                        {
                            ExtractFullPath = extractFullPath,
                            Overwrite = overrite
                        });
                    }
                }
            }
        }

    }
    public enum CompressionFormats
    {
        zip,
        gzip,
        tar
    }
}
