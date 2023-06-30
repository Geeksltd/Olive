namespace Olive.Migration.Services
{
    using System;
    using System.Threading.Tasks;
    using Olive.Migration.Services.Contracts;

    public class RestoreService : IRestoreService
	{
		public async Task<(bool success, string errorMessage)> Restore(string path)
		{
			return (true,"");
		}
	}
}