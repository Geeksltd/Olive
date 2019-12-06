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
using AppContentService;
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
    [Authorize(Roles = "Director")]
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class AdminServicesController : BaseController
    {
        [Route("admin/services")]
        public async Task<ActionResult> Index(vm.ServicesList info)
        {
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            
            return View(info);
        }
        
        [NonAction, OnBound]
        public async Task OnBound(vm.ServicesList info)
        {
            info.Items = await GetSource(info)
                .Select(item => new vm.ServicesList.ListItem(item)).ToList();
        }
        
        [NonAction]
        async Task<IEnumerable<Service>> GetSource(vm.ServicesList info)
        {
            IEnumerable<Service> result = Service.All;
            
            if (info.FullSearch.HasValue())
            {
                var keywords = info.FullSearch.Split(' ').Trim().ToArray();
                result = result.Where(item => item.ToString("F").ContainsAll(keywords, caseSensitive: false));
            }
            
            return result;
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class ServicesList : IViewModel
    {
        // Search filters
        public string FullSearch { get; set; }
        
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