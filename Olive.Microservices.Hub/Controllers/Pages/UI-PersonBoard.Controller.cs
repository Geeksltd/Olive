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
    [Authorize(Roles = "Employee")]
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class UIPersonBoardController : BaseController
    {
        [Route("person/{featureId}")]
        public async Task<ActionResult> Index(vm.BoardView info)
        {
            ViewBag.Info = info;
            ViewData["LeftMenu"] = "FeaturesSideMenu";
            
            return View(ViewBag);
        }
        
        [NonAction, OnPreBound]
        public async Task OnBinding(vm.BoardView info)
        {
            info.Item = Board.Parse("Person");
        }
    }
}