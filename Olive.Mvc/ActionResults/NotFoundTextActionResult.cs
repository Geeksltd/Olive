namespace Olive.Mvc
{
    using System.Net;

    public class NotFoundTextActionResult : TextActionResult
    {
        public NotFoundTextActionResult(string message) : base(message, HttpStatusCode.NotFound) { }
    }
}