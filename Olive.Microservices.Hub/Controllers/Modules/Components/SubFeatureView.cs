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
    public partial class SubFeatureView : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.SubFeatureView info)
        {
            return View(await Bind<vm.SubFeatureView>(info));
        }
    }
}

namespace Controllers
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class SubFeatureViewController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.SubFeatureView info)
        {
            // Load Javascript file
            JavaScript(new JavascriptService("hub", "go", info.RedirectUrl, info.Item.UseIframe));
        }
        
        protected override async Task<bool> AuthorizeRequestParams(ActionExecutingContext context)
        {
            if (!(User.Identity.IsAuthenticated))
                return false;
            
            return await base.AuthorizeRequestParams(context);
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    [BindingController(typeof(Controllers.SubFeatureViewController))]
    public partial class SubFeatureView : IViewModel
    {
        [ReadOnly(true)]
        public string SubFeatureImplementationUrl { get; set; }
        
        public string RedirectUrl
        {
            get
            {
                return Item.ToHubSubFeatureUrl(SubFeatureImplementationUrl);
            }
        }
        
        [ValidateNever]
        public Feature Item { get; set; }
    }
}