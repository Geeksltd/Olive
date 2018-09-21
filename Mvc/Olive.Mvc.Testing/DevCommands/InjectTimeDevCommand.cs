using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class InjectTimeDevCommand : DevCommand
    {
        public InjectTimeDevCommand(IHttpContextAccessor contextAccessor) : base(contextAccessor) { }

        public override string Name => "local-date";

        public override async Task<bool> Run()
        {
            if (Param("date") == "now")
            {
                // reset to normal
                LocalTime.RedefineNow(overriddenNow: null);
                await Context.Response.EndWith(LocalTime.Now.ToString("yyyy-MM-dd @ HH:mm:ss"));
            }
            else
            {
                var date = Param("date").TryParseAs<DateTime>() ?? LocalTime.Today;
                var time = Param("time").TryParseAs<TimeSpan>() ?? LocalTime.Now.TimeOfDay;

                date = date.Add(time);

                var trueOrigin = DateTime.Now;

                LocalTime.RedefineNow(() => date.Add(DateTime.Now.Subtract(trueOrigin)));
                await Context.Response.EndWith(date.ToString("yyyy-MM-dd @ HH:mm:ss"));
            }

            return true;
        }
    }
}
