//namespace MSharp.Framework.Mvc
//{
//    using System;
//    using System.Security.Claims;
//    using System.Security.Principal;
//    using System.Web;
//    using System.Web.Mvc;
//    using System.Web.Routing;
//    using MSharp.Framework.UI;

//    /// <summary>
//    /// Provides a base http application class for MSharp MVC applications.
//    /// </summary>
//    public abstract class HttpApplication : BaseHttpApplication
//    {
//        /// <summary>
//        /// Sets up the standard providers and configures the application.
//        /// </summary>
//        protected virtual void Application_Start()
//        {
//            GlobalFilters.Filters.Add(new HandleErrorAttribute());
//            GlobalFilters.Filters.Add(new JsonHandlerAttribute());

//            MvcHandler.DisableMvcResponseHeader = true;

//            RegisterRoutes();

//            RegisterBinders();

//            Document.SecureVirtualRoot = "/file/download?";
//        }

//        /// <summary>
//        /// Registers the standard Routes.
//        /// </summary>
//        protected virtual void RegisterRoutes()
//        {
//            RouteTable.Routes.MapMvcAttributeRoutes();
//            RouteTable.Routes.MapRoute("Default", "{controller}/{action}");
//            RegisterNotFoundRoute();


//        }


//        /// <summary>
//        /// Retrieves the actual user object from the principal info.
//        /// </summary>
//        protected IPrincipal RetrieveActualUser<TUser, TAnonymousUser>(IPrincipal principal)
//            where TUser : IEntity, IPrincipal
//            where TAnonymousUser : IEntity, IPrincipal, new()
//        {
//            if (principal == null) return new TAnonymousUser();
//            if (!principal.Identity.IsAuthenticated) return new TAnonymousUser();

//            var asIdentity = principal.Identity as ClaimsIdentity;
//            if (asIdentity != null)
//            {
//                var result = Database.GetOrDefault<TUser>(asIdentity.Name) as IPrincipal;
//                return result ?? new TAnonymousUser();
//            }

//            throw new IdentityNotMappedException("I do not recognise " + principal.Identity.GetType().FullName + " identity for authentication.");
//        }

//    }
//}