namespace Olive.WebApi
{
    public class AuthorizeUserAttribute : AuthorizationFilterAttribute
    {
        public override bool AllowMultiple => false;

        public override void OnAuthorization(HttpActionContext actionContext) { }
    }
}