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
    public partial class FeaturesSideMenu : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.FeaturesSideMenu info)
        {
            return View(await Bind<vm.FeaturesSideMenu>(info));
        }
    }
}

namespace Controllers
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class FeaturesSideMenuController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesSideMenu info)
        {
            info.Markup = (await AuthroziedFeatureInfo.RenderMenu(Website.FeatureContext.ViewingFeature)).ToString();
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    [BindingController(typeof(Controllers.FeaturesSideMenuController))]
    public partial class FeaturesSideMenu : IViewModel
    {
        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}