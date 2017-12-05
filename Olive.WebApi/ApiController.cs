namespace Olive.WebApi
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Principal;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Olive.Web;
    using System.Threading.Tasks;

    [RespectClientETagCache]
    public class ApiController : Controller
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            if (User == null || User is WindowsPrincipal || User.IsInRole("Anonymous"))
                await JwtAuthenticate();
        }

        public NotFoundTextActionResult NotFound(string message) => new NotFoundTextActionResult(message);

        public UnauthorizedTextActionResult Unauthorized(string message) => new UnauthorizedTextActionResult(message);

        /// <summary>
        /// Dispatches the specified file.
        /// </summary>
        protected async Task<HttpResponseMessage> File(FileInfo file)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(await file.ReadAllBytes())
            };

            result.Content.Headers.Perform(h =>
            {
                h.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = file.Name };
                h.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            });

            return result;
        }

        protected async Task JwtAuthenticate()
        {
            var user = await JwtAuthentication.ExtractUser(Request.Headers);

            if (user != null && !(user is IPrincipal))
                throw new Exception("User should implement IPrincipal.");

            Context.Http.User = user as IPrincipal;
        }
    }
}