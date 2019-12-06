using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Olive;
using Olive.Entities;
using Olive.Mvc;
using Olive.Web;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PeopleService;
using vm = ViewModel;

namespace ViewComponents
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class MainMenu : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.MainMenu info)
        {
            return View(await Bind<vm.MainMenu>(info));
        }
    }
}

namespace Controllers
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class MainMenuController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.MainMenu info)
        {
            info.ActiveItem = GetActiveItem(info);
        }
        
        [NonAction]
        public string GetActiveItem(vm.MainMenu info)
        {
            return null;
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    [BindingController(typeof(Controllers.MainMenuController))]
    public partial class MainMenu : IViewModel
    {
        public string ActiveItem { get; set; }
    }
}