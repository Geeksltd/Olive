namespace Olive.Migration.Controllers
{
	using Microsoft.AspNetCore.Mvc;
	using Olive.Migration;
	using Olive.Migration.Services.Contracts;
	using System.Threading.Tasks;
	using ViewModel;

	[Route("olive/migration")]
	public class RestoreController : Olive.Mvc.Controller
	{
		[HttpGet("restore/{Id}")]
		public async Task<IActionResult> Index(RestoreRequest request)
		{
			var item = await Database.Get<IMigrationTask>(request.Id);

			var vm = new Restore
			{
				Item = item
			};

			return View(vm);
		}

		[HttpPost("restore/{Id}")]
		public async Task<IActionResult> RestoreResult([FromServices] IMigrationService migrationService, RestoreRequest request)
		{
			var item = await Database.Get<IMigrationTask>(request.Id);
			var (task, errorMessage) = await migrationService.Restore(item,request.Witch == RestoreWhich.Before);

			var vm = new Restore
			{
				Item = task,
				ErrorMessage = errorMessage
			};

			return View("RestoreResult", vm);
		}
	}
}

namespace ViewModel
{
	using Olive.Mvc;
	using Olive.Migration;
	using System;
	using Microsoft.AspNetCore.Mvc;

	public class RestoreRequest
	{
		[FromRoute]
		public Guid Id { get;set;}
		[FromQuery]
		public RestoreWhich Witch { get;set;}
	}

	public enum RestoreWhich
	{
		Before,After
	}

	public class Restore : IViewModel
	{
		public string ErrorMessage = "";
		public IMigrationTask Item = null;
	}
}