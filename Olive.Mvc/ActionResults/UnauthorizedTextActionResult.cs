namespace Olive.Mvc
{
    using System.Net;

    public class UnauthorizedTextActionResult : TextActionResult
    {
        public UnauthorizedTextActionResult(string message) : base(message, HttpStatusCode.Unauthorized) { }
    }
}
