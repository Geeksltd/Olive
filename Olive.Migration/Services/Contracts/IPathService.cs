namespace Olive.Migration.Services.Contracts
{
    using System.IO;

    public interface IPathService
    {
        DirectoryInfo MigrationDirectory();
        FileInfo MigrationFile(string fileName);
        FileInfo[] MigrationFiles();
    }
}