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
    public partial class ManualLogin : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(vm.ManualLogin info)
        {
            return View(await Bind<vm.ManualLogin>(info));
        }
    }
}

namespace Controllers
{
    [EscapeGCop("Auto generated code.")]
#pragma warning disable
    public partial class ManualLoginController : BaseController
    {
        [HttpPost("ManualLogin/SimulateLogIn")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SimulateLogIn(vm.ManualLogin info)
        {
            if (!(info.IsVisible))
                return new UnauthorizedResult();
            await info.CopyDataTo(info.Item);

            info.Item.Roles = info.RoleNames;

            await info.Item.LogOn();

            return Redirect(Request.Param("returnUrl"));
        }

        [NonAction, OnPreBinding]
        public async Task OnPreBinding(vm.ManualLogin info)
        {
            if (Request.IsGet())
            {
                // Set default roles
                info.RoleNames = @"Employee,Dev,JuniorDev,SeniorDev,LeadDev,HeadDev,QA,JuniorQA,SeniorQA,LeadQA,HeadQA,BA,JuniorBA,SeniorBA,LeadBA,HeadBA,PM,JuniorPM,SeniorPM,LeadPM,HeadPM,AM,JuniorAM,SeniorAM,LeadAM,HeadAM,Director,JuniorDirector,SeniorDirector,LeadDirector,HeadDirector,Designer,JuniorDesigner,SeniorDesigner,LeadDesigner,HeadDesigner,IT,JuniorIT,SeniorIT,LeadIT,HeadIT,Reception,JuniorReception,SeniorReception,LeadReception,HeadReception,PA,JuniorPA,SeniorPA,LeadPA,HeadPA,Sales,JuniorSales,SeniorSales,LeadSales,HeadSales";
            }
        }

        [NonAction, OnBound]
        public async Task OnBound(vm.ManualLogin info)
        {
            info.Item = info.Item ?? new UserInfo();

            info.IsVisible = info.AllowManual;

            if (Request.IsGet()) await info.Item.CopyDataTo(info);

            info.DisplayName = "Paymon Khamooshi";
            info.Email = "paymon@geeks.ltd.uk";
            info.Item.ID = "69D81564-809D-45A8-83CC-D12971A008D5".To<Guid>();


            info.DisplayName_Visible = info.IsVisible;
            info.Email_Visible = info.IsVisible;
            info.RoleNames_Visible = info.IsVisible;

            TryValidateModel(info);
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
#pragma warning disable
    [BindingController(typeof(Controllers.ManualLoginController))]
    public partial class ManualLogin : IViewModel
    {
        [ReadOnly(true)]
        public bool IsVisible { get; set; }

        public bool AllowManual
        {
            get
            {
                return Config.Get("Authentication:AllowManual", defaultValue: false);
            }
        }

        [ValidateNever]
        public UserInfo Item { get; set; }

        [CustomBound]
        public string DisplayName { get; set; }

        [CustomBound]
        [StringLengthWhen(nameof(Email_Visible), 100, ErrorMessage = "Email should not exceed 100 characters.")]
        public string Email { get; set; }

        public string RoleNames { get; set; }

        [ReadOnly(true)]
        public bool DisplayName_Visible { get; set; }

        [ReadOnly(true)]
        public bool Email_Visible { get; set; }

        [ReadOnly(true)]
        public bool RoleNames_Visible { get; set; }
    }
}