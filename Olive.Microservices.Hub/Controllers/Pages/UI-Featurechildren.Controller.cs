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
    public partial class UIFeaturechildrenController : BaseController
    {
        [Route("/feature/children/{parent}")]
        [Route("/root")]
        [Route("/under")]
        public async Task<ActionResult> Index(vm.ChildFeaturesList info)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = Url.Current() }));
            }
            
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            
            return View(info);
        }
        
        [NonAction, OnBound]
        public async Task OnBound(vm.ChildFeaturesList info)
        {
            // Set browser title
            ViewData["Title"] = info.Parent.ToStringOrEmpty().Or("Home");
            
            info.Items = await GetSource(info)
                .Select(item => new vm.ChildFeaturesList.ListItem(item)).ToList();
        }
        
        [NonAction]
        async Task<IEnumerable<Feature>> GetSource(vm.ChildFeaturesList info)
        {
            IEnumerable<Feature> result = Feature.All.Where(x => x.Parent == info.Parent && x.Title != "WIDGETS").Where(item => Context.Current.User().CanSee(item));
            
            if (info.InstantSearch.HasValue())
            {
                var keywords = info.InstantSearch.Split(' ').Trim().ToArray();
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
    public partial class ChildFeaturesList : IViewModel
    {
        [ValidateNever]
        public Feature Parent { get; set; }
        
        // Search filters
        public string InstantSearch { get; set; }
        
        [ReadOnly(true)]
        public List<ListItem> Items = new List<ListItem>();
        
        public partial class ListItem : IViewModel
        {
            public ListItem(Feature item) => Item = item;
            
            [ValidateNever]
            public Feature Item { get; set; }
        }
    }
}