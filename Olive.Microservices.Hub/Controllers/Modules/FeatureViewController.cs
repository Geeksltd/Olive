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
    public partial class FeatureViewController : BaseController
    {
        [NonAction, OnBound]
        public async Task OnBound(vm.FeatureView info)
        {
            // Load Javascript file
            JavaScript(new JavascriptService("hub", "go", info.Item.LoadUrl, info.Item.UseIframe));
            
            if (info.Item?.ImplementationUrl.HasValue() == true && info.Item?.UseIframe == true)
            {
                // Load Javascript file
                JavaScript(new JavascriptService("featuresMenu", "show", info.Item.ID));
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
    [BindingController(typeof(Controllers.FeatureViewController))]
    public partial class FeatureView : IViewModel
    {
        [ValidateNever]
        public Feature Item { get; set; }
    }
}