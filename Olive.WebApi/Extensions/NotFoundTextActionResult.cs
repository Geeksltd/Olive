namespace Olive.WebApi
{
    using Microsoft.AspNetCore.Mvc;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class NotFoundTextActionResult : TextActionResult
    {
        public NotFoundTextActionResult(string message) : base(message, HttpStatusCode.NotFound) { }
    }
}