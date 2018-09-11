using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Olive.Entities.Data.SqlServer.Tests
{
    [TestFixture]
    public partial class EntityDatabaseTests
    {
        IDatabase database;
        string TempDatabaseName;
        DirectoryInfo DbDirectory, DatabaseFilesPath;

        public static DirectoryInfo DatabaseStoragePath
        {
            get
            {
                var result = Config.GetOrThrow("Database:StoragePath").AsDirectory();

                if (!result.Exists())
                {
                    // Try to build once:
                    try { result.Create(); }
                    catch
                    {
                        throw new Exception($"Failed to create the folder '{result.FullName}'. " +
                            "Ensure it exists and is accessible to the current ASP.NET process. " +
                            $"Otherwise specify a different location in AppSetting of 'Database:StoragePath'.");
                    }
                }

                return result;
            }
        }

        Dictionary<FileInfo, string> GetExecutableCreateDbScripts(string tempDatabaseName)
        {
            var sources = GetCreateDbFiles();

            var result = new Dictionary<FileInfo, string>();

            foreach (var file in sources)
            {
                var script = File.ReadAllText(file.FullName);

                // The first few lines contain #DATABASE.NAME# which should be replaced.
                script = script.ToLines().Select((line, index) =>
                {
                    if (index < 10)
                    {
                        return line
                            .Replace("#DATABASE.NAME#", tempDatabaseName)
                            .Replace("#STORAGE.PATH#", DatabaseFilesPath.FullName);
                    }

                    return line;
                }).ToLinesString();

                result.Add(file, script);
            }

            return result;
        }

        FileInfo[] GetCreateDbFiles()
        {
            if (DbDirectory == null) LoadMetaDirectory();

            var potentialSources = new List<FileInfo>();

            var tableScripts = DbDirectory.GetSubDirectory("Tables").GetFilesOrEmpty("*.sql");

            // Create tables:
            potentialSources.Add(DbDirectory.GetFile("@Create.Database.sql"));
            potentialSources.AddRange(tableScripts.Except(x => x.Name.ToLower().EndsWithAny(".fk.sql", ".data.sql")));

            // Insert data:
            potentialSources.Add(DbDirectory.GetFile("@Create.Database.Data.sql"));
            potentialSources.AddRange(tableScripts.Where(x => x.Name.ToLower().EndsWith(".data.sql")));

            potentialSources.Add(DbDirectory.GetFile("Customize.Database.sql"));

            // Add foreign keys            
            potentialSources.AddRange(tableScripts.Where(x => x.Name.ToLower().EndsWith(".fk.sql")));

            var sources = potentialSources.Where(f => f.Exists()).ToArray();

            if (sources.None())
                throw new Exception("Failed to find the SQL creation script files at:\r\n" + potentialSources.ToLinesString());

            return sources;
        }

        void LoadMetaDirectory()
        {
            DbDirectory = AppDomain.CurrentDomain.WebsiteRoot().GetSubDirectory("DB");
            if (!DbDirectory.Exists())
                throw new Exception("Failed to find the DB folder from which to create the temp database: " +
                    DbDirectory.FullName);
        }

    }
}
