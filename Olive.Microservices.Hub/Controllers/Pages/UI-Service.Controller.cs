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

namespace Controllers
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class UIServiceController : BaseController
    {
        [Route("/UI/Service/{service}")]
        public async Task<ActionResult> Index(vm.ServiceView info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }
            
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            
            return View(info);
        }
        
        [NonAction, OnBound]
        public async Task OnBound(vm.ServiceView info)
        {
            if (info.Url.HasValue())
            {
                // Load Javascript file
                JavaScript(new JavascriptService("hub", "go", info.DestinationUrl, info.Item.UseIframe));
            }
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
    public partial class ServiceView : IViewModel
    {
        [ReadOnly(true)]
        public string Url { get; set; }
        
        public string ActualRelativeUrl
        {
            get
            {
                return Url.ToLower().TrimStart($"/{ Item.Name.ToLower()}/");
            }
        }
        
        public string DestinationUrl
        {
            get
            {
                return Item.GetHubImplementationUrl(ActualRelativeUrl);
            }
        }
        
        [FromRequest("service")]
        [ValidateNever]
        public Service Item { get; set; }
    }
}