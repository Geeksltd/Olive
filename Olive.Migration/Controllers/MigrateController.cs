namespace Olive.Migration.Controllers
{
	using Microsoft.AspNetCore.Mvc;
	using Olive.Migration;
	using Olive.Migration.Services.Contracts;
	using System.Threading.Tasks;
	using ViewModel;

	[Route("olive/migration")]
	public class MigrateController : Olive.Mvc.Controller
	{
		public MigrateController() { }

		[HttpGet("migrate/{id}")]
		public async Task<IActionResult> Index([FromRoute] string id)
		{
			var item = await Database.Get<IMigrationTask>(id);

			var vm = new Migrate
			{
				Item = item
			};

			return View(vm);
		}

		[HttpPost("migrate/{id}")]
		public async Task<IActionResult> Migrate([FromServices] IMigrationService migrationService, [FromRoute] string id)
		{
			var item = await Database.Get<IMigrationTask>(id);
			var (task, errorMessage) = await migrationService.Migrate(item);

			var vm = new Migrate
			{
				Item = task,
				ErrorMessage = errorMessage
			};

			return View("MigrateResult", vm);
		}
	}
}

namespace ViewModel
{
	using Olive.Mvc;
	using Olive.Migration;

	public class Migrate : IViewModel
	{
		public string ErrorMessage = "";
		public IMigrationTask Item = null;
	}
}