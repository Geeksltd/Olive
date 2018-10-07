using System;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class InjectTimeDevCommand : DevCommand
    {
        public override string Name => "local-date";

        public override async Task<string> Run()
        {
            if (Param("date") == "now")
            {
                // reset to normal
                LocalTime.RedefineNow(overriddenNow: null);
                return LocalTime.Now.ToString("yyyy-MM-dd @ HH:mm:ss");
            }
            else
            {
                var date = Param("date").TryParseAs<DateTime>() ?? LocalTime.Today;
                var time = Param("time").TryParseAs<TimeSpan>() ?? LocalTime.Now.TimeOfDay;

                date = date.Add(time);

                var trueOrigin = DateTime.Now;

                LocalTime.RedefineNow(() => date.Add(DateTime.Now.Subtract(trueOrigin)));
                return date.ToString("yyyy-MM-dd @ HH:mm:ss");
            }
        }
    }
}
