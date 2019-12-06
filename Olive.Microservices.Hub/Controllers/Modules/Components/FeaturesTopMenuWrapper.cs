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
    public partial class FeaturesTopMenuWrapper : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.FeaturesTopMenuWrapper info)
        {
            return View(await Bind<vm.FeaturesTopMenuWrapper>(info));
        }
    }
}

namespace Controllers
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class FeaturesTopMenuWrapperController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.FeaturesTopMenuWrapper info)
        {
            info.Markup = (await AuthroziedFeatureInfo.RenderMenuJson()).ToString();
            
            info.IsVisible = User.Identity.IsAuthenticated;
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    [BindingController(typeof(Controllers.FeaturesTopMenuWrapperController))]
    public partial class FeaturesTopMenuWrapper : IViewModel
    {
        [ReadOnly(true)]
        public bool IsVisible { get; set; }
        
        [ReadOnly(true)]
        public string Markup { get; set; }
    }
}