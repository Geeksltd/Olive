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
    public partial class SSOController : BaseController
    {
        [Route("sso")]
        public async Task<ActionResult> Index(vm.SingleSignOn info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }
            
            return View(info);
        }
        
        [NonAction, OnBound]
        public async Task OnBound(vm.SingleSignOn info)
        {
            info.Token = CreateToken(info.Ticket);
            
            info.Items = await GetSource(info)
                .Select(item => new vm.SingleSignOn.ListItem(item)).ToList();
            
            // Prepare apps
            await PrepareApps(info);
        }
        
        [NonAction]
        async Task<IEnumerable<Service>> GetSource(vm.SingleSignOn info)
        {
            IEnumerable<Service> result = Service.All.Where(x => x.InjectSingleSignon);
            
            return result;
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class SingleSignOn : IViewModel
    {
        [ReadOnly(true)]
        public string Token { get; set; }
        
        [ReadOnly(true)]
        public List<ListItem> Items = new List<ListItem>();
        
        public partial class ListItem : IViewModel
        {
            public ListItem(Service item) => Item = item;
            
            [ValidateNever]
            public Service Item { get; set; }
            
            public string ServiceUrl
            {
                get
                {
                    return Item.GetAbsoluteImplementationUrl("@Services/SSO.ashx");
                }
            }
        }
    }
}