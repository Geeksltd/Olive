namespace Olive.Migration.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Olive.Migration.Services.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using ViewModel;

    [Route("olive/migration/list")]
    public class ListController : Olive.Mvc.Controller
    {
		public ListController() { }

        public async Task<IActionResult> Index([FromServices] IMigrationListService migrationListService)
        {
			var vm = new MigrationsList();
			var (migrations ,errors)=await migrationListService.Get();
			vm.Items = migrations.Select(a=>new MigrationsList.MigrationItem(a)).ToList();
			vm.Errors = errors;

			return View(vm);
        }
    }
}

namespace ViewModel
{
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
    using Olive.Mvc;
    using System.Collections.Generic;
    using Olive.Migration;

    public  class MigrationsList : IViewModel
	{
		public List<MigrationItem> Items = new List<MigrationItem>();

		public List<string> Errors = new List<string>();
		
		public  class MigrationItem : IViewModel
		{
			public MigrationItem(IMigrationTask item) => Item = item;

			[ValidateNever]
			public IMigrationTask Item { get; set; }
		}
	}
}