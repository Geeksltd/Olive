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
    [Authorize(Roles = "Director, HeadPM")]
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class AdminServicesHealthCheckController : BaseController
    {
        [Route("admin/services-health-check")]
        public async Task<ActionResult> Index(vm.ServicesHealthCheckTiles info)
        {
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            
            return View(info);
        }
        
        [NonAction, OnBound]
        public async Task OnBound(vm.ServicesHealthCheckTiles info)
        {
            info.Items = await GetSource(info)
                .Select(item => new vm.ServicesHealthCheckTiles.ListItem(item)).ToList();
        }
        
        [NonAction]
        async Task<IEnumerable<Service>> GetSource(vm.ServicesHealthCheckTiles info)
        {
            IEnumerable<Service> result = Service.All;
            
            return result;
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class ServicesHealthCheckTiles : IViewModel
    {
        [ReadOnly(true)]
        public List<ListItem> Items = new List<ListItem>();
        
        public partial class ListItem : IViewModel
        {
            public ListItem(Service item) => Item = item;
            
            [ValidateNever]
            public Service Item { get; set; }
        }
    }
}