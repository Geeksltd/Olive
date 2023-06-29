namespace Olive.Migration.Services
{
    using System;
    using System.Threading.Tasks;
    using Olive.Migration.Services.Contracts;

    public class RestoreService : IRestoreService
	{
		public Task<(bool success, string errorMessage)> Restore()
		{
			throw new NotImplementedException();
		}
	}
}