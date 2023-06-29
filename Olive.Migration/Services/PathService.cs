﻿namespace Olive.Migration.Services
{
    using Microsoft.AspNetCore.Hosting;
    using Olive.Entities;
    using Olive.Migration.Services.Contracts;
    using System.IO;
    using System.Linq;

    public class PathService : IPathService
	{
		private readonly IWebHostEnvironment _environment;


		public PathService(IWebHostEnvironment environment)
		{
			_environment = environment;
		}

		public DirectoryInfo MigrationDirectory()
		{
			var directory = Path.Combine(_environment.ContentRootPath, "Migrations");
			return new DirectoryInfo(directory);
		}

		public FileInfo MigrationFile(string fileName)
		{
			return MigrationDirectory().GetFile(fileName + ".sql");
		}

		public FileInfo[] MigrationFiles()
		{
			var directory = MigrationDirectory();

			if (!directory.Exists()) { return null; }

			var files = directory.GetFiles("*.sql");

			if (files.None()) { return null; }

			files = files.OrderBy(a => a.Name).ToArray();

			return files;
		}
	}
}