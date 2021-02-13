using System;
using System.IO;

namespace Olive.Utilities
{
    public class TemporaryDirectory : IDisposable
    {
        static readonly string TemporaryFileFolder = Path.GetTempPath();

        Guid ID;

        public string Extension { get; set; }

        /// <summary>
        /// Creates a new instance of temporary file. The file will have "dat" extension by default.
        /// </summary>
        public TemporaryDirectory()
        {

        }

        /// <summary>
        /// Gets or sets the FilePath of this TemporaryFile.
        /// </summary>
        public string FilePath
        {
            get
            {
                var folder = TemporaryFileFolder.AsDirectory().CreateSubdirectory(ID.ToString());
                return folder.FullName;
            }
        }

        

        /// <summary>
        /// Disposes this instance of temporary file and deletes the file if provided
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (FilePath.AsDirectory().Exists()) Directory.Delete(FilePath,true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex, "Can not dispose temporary directory.");
            }
        }
    }
}
