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
using Olive.Security;
using Olive.Web;
using Domain;
using Microsoft.AspNetCore.Authentication;
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
    public partial class LoginController : BaseController
    {
        [Route("login/{item:Guid?}")]
        public async Task<ActionResult> Index(vm.ManualLogin manualLogin, vm.LoginForm loginForm)
        {
            if (Request.Param("returnUrl").IsEmpty())
            {
                return Redirect(Url.Index("Login", new { ReturnUrl = "/" }));
            }
            
            // Remove initial validation messages as well as unintended injected data
            ModelState.Clear();
            
            ViewBag.ManualLogin = manualLogin;
            
            return View(loginForm);
        }
        
        [HttpPost("LoginForm/LoginByGoogle")]
        public async Task<ActionResult> LoginByGoogle(vm.LoginForm info)
        {
            await OAuth.Instance.LoginBy("Google");
            
            return JsonActions(info);
        }
        
        [NonAction, OnBound]
        public async Task OnBound(vm.LoginForm info)
        {
            info.Item = info.Item ?? new User();
            
            // Clear cookies
            var alreadyDead = new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = LocalTime.Today.AddDays(-1)
            };
            
            foreach (var c in Request.Cookies)
                Response.Cookies.Append(c.Key, string.Empty, alreadyDead);
            
            if (Request.IsGet()) await info.Item.CopyDataTo(info);
        }
    }
}

namespace ViewModel
{
    [EscapeGCop("Auto generated code.")]
    #pragma warning disable
    public partial class LoginForm : IViewModel
    {
        [ReadOnly(true)]
        public string ErrorMessage { get; set; }
        
        [ValidateNever]
        public User Item { get; set; }
    }
}