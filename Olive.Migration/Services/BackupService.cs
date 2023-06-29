namespace Olive.Migration.Services
{
    using System;
    using System.Threading.Tasks;
    using Olive.Migration.Services.Contracts;

    public class BackupService : IBackupService
	{
		public Task<(bool success, string path, string errorMessage)> Backup()
		{
			throw new NotImplementedException();
		}
	}
}