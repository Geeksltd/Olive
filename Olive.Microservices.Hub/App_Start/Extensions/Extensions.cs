using Microsoft.AspNetCore.Http;
using Olive;
using PeopleService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class Extensions
    {
        public static Task LogOn(this UserInfo @this)
        {
            return new Olive.Security.GenericLoginInfo
            {
                DisplayName = @this.DisplayName,
                Email = @this.Email,
                ID = @this.ID.ToString(),
                Roles = @this.Roles.Split(',').Trim().ToArray(),
                Timeout = 2.Hours()
            }.LogOn();
        }

        public static Domain.Feature[] SubItems(this Domain.Feature @this)
        {
            if (@this == null)
                return Domain.Feature.All.Where(x => x.Parent == null).ToArray();
            else return @this.GetAllChildren().Cast<Domain.Feature>().ToArray();
        }

        public static bool IsSmartPhone(this HttpRequest @this)
        {
            var agent = @this.Headers["User-Agent"].ToString("|").ToLowerOrEmpty();
            return agent.ContainsAny(new[] { "iphone", "android" });
        }
    }
}