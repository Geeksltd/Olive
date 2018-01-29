namespace Olive.Mvc
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public abstract class TextActionResult : IActionResult
    {
        public string Message { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        protected TextActionResult(string message, HttpStatusCode statusCode)
        {
            Message = message;
            StatusCode = statusCode;
        }

        public virtual async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)StatusCode;

            var body = Message.GetUtf8WithSignatureBytes();

            await context.HttpContext.Response.Body.WriteAsync(body, 0, body.Length);
        }
    }
}