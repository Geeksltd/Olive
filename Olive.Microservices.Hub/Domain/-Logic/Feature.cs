using Olive;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain
{
    partial class Feature
    {
        public static IEnumerable<Feature> All { get; internal set; }

        public IHierarchy GetParent() => Parent;

        public IEnumerable<IHierarchy> GetChildren() => Children;

        string IHierarchy.Name => Title;

        public static Feature FindByHubUrl(string path)
        {
            return All
                .Where(f => f.GetHubUrl().Equals(path, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(x => Context.Current.User().CanSee(x));
        }

        public static Feature FindByLoadUrl(string url)
        {
            url = url.ToLower().EnsureStartsWith("/");
            return All.Where(x => x.LoadUrl == url).WithMax(x => Context.Current.User().CanSee(x));
        }

        public static Feature FindByRef(string @ref)
        {
            if (@ref.IsEmpty()) return null;
            return All.FirstOrDefault(x => x.Ref == @ref)
                ?? throw new Exception("Feature not found: " + @ref);
        }

        public static Feature FindBySubFeaturePath(string path)
        {
            path = path.TrimStart("/").ToLower();

            var potentialFeatures = from f in All
                                    let hubUrl = f.GetHubUrl().ToLower()
                                    where path.StartsWith(hubUrl)
                                    orderby Context.Current.User().CanSee(f) descending,
                                    hubUrl.Length descending
                                    select f;

            return potentialFeatures.FirstOrDefault();
        }

        public static Feature FindByAbsoluteImplementationUrl(string path)
        {
            return All.Where(f => f.GetAbsoluteImplementationUrl()
            .OrEmpty().Equals(path, StringComparison.OrdinalIgnoreCase))
            .WithMax(x => Context.Current.User().CanSee(x));
        }

        public async Task<IEnumerable<Feature>> GetSubFeatures()
        {
            // TODO: Apply restriction based on the current user:
            return Children;
        }

        public bool IsParent() => Children.Any();

        public string GetBadgeUrl()
        {
            if (BadgeUrl.IsEmpty()) return null;

            if (BadgeUrl.ToLower().StartsWithAny("//", "http://", "https://"))
                return BadgeUrl;
            else
                return Service.BaseUrl.TrimEnd("/") + BadgeUrl.EnsureStartsWith("/");
        }

        internal bool IsBadgeOptional()
        {
            if (BadgeUrl.IsEmpty() || BadgeOptionalFor.IsEmpty()) return false;
            var user = Context.Current.User();
            if (user == null) return false;

            foreach (var r in BadgeOptionalFor.Split(',').Trim())
            {
                var hasTheRole = user.IsInRole(r.TrimStart("!"));
                if (r.StartsWith("!"))
                {
                    if (!hasTheRole) return true;
                }
                else if (hasTheRole) return true;
            }

            return false;
        }

        public bool HasSecondLevelMenu()
        {
            var isFirstLevel = Parent != null;
            if (isFirstLevel && ImplementationUrl.HasValue()) return false;

            var isSubSideMenu = Parent != null && GrandParent == null;
            if (isSubSideMenu)
            {
                if (Children.Any()) return true;

                return ImplementationUrl.HasValue();
            }

            return true;
        }

        public string GetHubUrl() => Service.Name + ImplementationUrl;

        internal string FindLoadUrl()
        {
            if (ImplementationUrl.HasValue())
                ImplementationUrl = ImplementationUrl.EnsureStartsWith("/");

            if (UseIframe && Service.Name != "Hub") return "/[" + Service.Name + "]" + ImplementationUrl;

            if (ImplementationUrl.HasValue())
                return "/[" + Service.Name + "]" + ImplementationUrl;

            return "/under/" + this.WithAllParents().Reverse().Select(x => x.Title.ToPascalCaseId()).ToString("/").ToLower();
        }

        public string GetAbsoluteImplementationUrl() => Service.GetAbsoluteImplementationUrl(ImplementationUrl);

        public string ToHubSubFeatureUrl(string url)
        {
            url = url.TrimStart("/").TrimStart(Service.Name + "/", caseSensitive: false);

            return Service.GetHubImplementationUrl(url);
        }

        public string GetTitle(Feature relativeTo)
            => ToString().Substring(relativeTo.ToStringOrEmpty().Length).TrimStart(" > ");

        public string GetIcon()
        {
            if (Icon.HasValue()) return Icon;
            else if (Children.Any()) return "fa-folder-open";
            else return "fa-window-maximize";
        }

        public string GetDescription()
        {
            if (Description.HasValue()) return Description;
            if (Children.None()) return string.Empty;

            return Children.Select(x => x.GetDescription().Or(x.Title)).Trim().Distinct().ToString(", ");
        }

        public class DataProvider : LimitedDataProvider
        {
            public static void Register()
            {
                Context.Current.Database().RegisterDataProvider(typeof(Feature), new DataProvider());
            }

            public override Task<IEntity> Get(object objectID)
            {
                var id = objectID.ToString().To<Guid>();
                return Task.FromResult((IEntity)All.First(x => x.ID == id));
            }
        }
    }
}