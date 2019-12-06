using Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Olive;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    class FeatureRequestHandler
    {
        HttpContext Context;
        Func<Task> Next;
        HttpRequest Request;
        string Path, HostAndPath, ExecutionUrl, NewQueryString;
        Feature Feature;
        Service Service;
        bool IsSubFeatureView;

        public FeatureRequestHandler(HttpContext context, Func<Task> next)
        {
            Context = context;
            Next = next;
            Request = context.Request;
            ReadPath();
        }

        void ReadPath()
        {
            Path = Request.ToPathAndQuery().TrimStart("/");

            var open = "%5B";
            var close = "%5D";

            if (Path.StartsWith(open) && Path.Contains(close))
            {
                Path = Path.Substring(3);
                Path = Path.Remove(Path.IndexOf(close), 3);
            }

            HostAndPath = Request.RootUrl() + Request.Path.ToString().TrimStart("/");

            if (Path.IsEmpty())
            {
                Path = "dashboard/home.aspx";
                HostAndPath = Request.RootUrl() + Path.TrimStart("/");
            }
        }

        internal async Task Handle()
        {
            if (IsStaticContentRequest())
            {
                await Next();
                return;
            }

            Website.FeatureContext.ViewingFeature = Feature = FindFeature();
            Service = FindService();

            if (Service == null)
            {
                await Next();
                return;
            }

            await FindExecutionUrl();
            await Execute();
        }

        bool IsStaticContentRequest()
        {
            return Request.Path.Value.ToLower().TrimStart("/")
                .StartsWithAny("lib", "img", "styles", "scripts", "healthcheck");
        }

        Feature FindFeature()
        {
            var result = Feature.All.FirstOrDefault(x => x.LoadUrl == Path.ToLower().EnsureStartsWith("/"));

            if (result == null)
                result = Feature.FindByHubUrl(Path) ?? Feature.FindByAbsoluteImplementationUrl(HostAndPath);

            if (result == null)
            {
                result = Feature.FindBySubFeaturePath(Path);
                if (result != null) IsSubFeatureView = true;
            }

            return result;
        }

        Service FindService()
        {
            if (Feature != null) return Feature.Service;

            return Service.All.FirstOrDefault(s =>
            Path.TrimStart("/").StartsWith(s.Name, caseSensitive: false));
        }

        async Task FindExecutionUrl()
        {
            if (!Context.User.Identity.IsAuthenticated)
            {
                ExecutionUrl = "/login";

                if (Context.Request.ToRawUrl().HasValue() && Context.Request.ToRawUrl() != "/")
                {
                    NewQueryString = $"returnUrl={Context.Request.ToRawUrl().UrlEncode()}";
                }
            }
            else if (Feature == null)
            {
                ExecutionUrl = "/UI/service/" + Service.ID;
                NewQueryString = "url=" + Path.TrimStart("/").Substring(Service.Name.Length).UrlEncode();
            }
            else if (!Context.User.CanSee(Feature))
            {
                ExecutionUrl = $"/UI/Unauthorized/{Feature.ID}";
            }
            else
            {
                if (IsSubFeatureView)
                    NewQueryString = "subFeatureImplementationUrl=" + Path.UrlEncode();

                if (Feature.ImplementationUrl.HasValue())
                {
                    if (IsSubFeatureView) ExecutionUrl = "/UI/subfeature/" + Feature.ID;
                    else if (Feature.Service.Name != "Hub") ExecutionUrl = "/UI/feature/" + Feature.ID;
                    else ExecutionUrl = Feature.ImplementationUrl;
                }
                else ExecutionUrl = "/feature/children/" + Feature.ID;
            }
        }

        async Task Execute()
        {
            var originalPath = Request.ToPathAndQuery();
            var originalQueryString = Request.QueryString;

            if (NewQueryString.HasValue())
                Request.QueryString = new QueryString(NewQueryString.WithPrefix("?"));

            Request.Path = ExecutionUrl.EnsureStartsWith("/");

            // Invoke the standard Mvc Controller for the rewritten path:
            await Next.Invoke();

            // Revert the rewritten url
            Request.QueryString = originalQueryString;
            Request.Path = originalPath;
        }
    }

    public static class FeatureRequestHandlerExtensions
    {
        public static IApplicationBuilder UseFeaturesViewPageMiddleware(this IApplicationBuilder @this)
        {
            return @this.Use((context, next) => new FeatureRequestHandler(context, next).Handle());
        }
    }
}