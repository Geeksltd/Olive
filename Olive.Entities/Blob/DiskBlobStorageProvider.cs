using System;
using System.IO;
using System.Threading.Tasks;

namespace Olive.Entities
{
    public class DiskBlobStorageProvider : IBlobStorageProvider
    {
        static DirectoryInfo root;

        /// <summary>
        /// Gets the physical path root.
        /// </summary>
        public static DirectoryInfo Root => root ?? (root = GetRoot(AppDomain.CurrentDomain.WebsiteRoot()));

        public static DirectoryInfo GetRoot(DirectoryInfo baseAddress)
        {
            var folder = Config.Get("Blob:RootPath", "Blob");

            if (!folder.StartsWith("\\\\") && folder[1] != ':') // Relative address:
                folder = baseAddress.GetSubDirectory(folder).FullName;

            return folder.AsDirectory();
        }

        static FileInfo File(Blob blob)
        {
            if (blob.OwnerEntity == null) return null;
            var folder = Folder(blob);
            return folder.GetFile(blob.OwnerId() + blob.FileExtension);
        }

        static DirectoryInfo Folder(Blob blob)
        {
            if (blob.OwnerEntity == null) return null;
            return Root.GetOrCreateSubDirectory(blob.FolderName);
        }

        public virtual async Task SaveAsync(Blob blob)
        {
            var path = File(blob);
            if (path == null) throw new InvalidOperationException("This blob is not linked to any entity.");

            var fileDataToSave = await blob.GetFileDataAsync(); // Because file data will be lost in delete.
            await DeleteAsync(blob);
            await path.WriteAllBytesAsync(fileDataToSave);
        }

        public virtual async Task DeleteAsync(Blob blob)
        {
            // Delete old file. TODO: Archive the files instead of deleting.
            foreach (var file in Folder(blob).GetFiles(blob.OwnerId() + ".*"))
                await file.DeleteAsync(harshly: true);
        }

        public virtual async Task<byte[]> LoadAsync(Blob blob)
        {
            var path = File(blob);
            if (path == null) return new byte[0];
            return await path.ReadAllBytesAsync();
        }

        public virtual Task<bool> FileExistsAsync(Blob blob) => File(blob).ExistsAsync();

        public virtual bool CostsToCheckExistence() => false;
    }
}