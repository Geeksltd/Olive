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
    public partial class UISubFeatureController : BaseController
    {
        [Route("UI/SubFeature/{item}")]
        public async Task<ActionResult> Index(vm.SubFeatureView info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }
            
            ViewBag.Info = info;
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            
            return View(ViewBag);
        }
        
        protected override async Task<bool> AuthorizeRequestParams(ActionExecutingContext context)
        {
            if (!(User.Identity.IsAuthenticated))
                return false;
            
            return await base.AuthorizeRequestParams(context);
        }
    }
}