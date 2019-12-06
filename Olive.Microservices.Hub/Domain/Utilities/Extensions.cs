namespace Domain
{
    using Olive;
    using Olive.Entities;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Provides the business logic for Extensions class.
    /// </summary>
    public static class Extensions
    {
        public static string GetFullPath(this IHierarchy @this, string separator = ">") => @this.WithAllParents().Select(i => i.ToString()).ToString($" {separator} ");

        public static bool CanSee(this ClaimsPrincipal @this, Feature feature)
        {
            foreach (var notPermission in feature.NotPermissions)
                if (@this.IsInRole(notPermission.Name)) return false;

            if (feature.Permissions.None())
            {
                if (feature.Parent != null)
                    return @this.CanSee(feature.Parent);

                return true;
            }

            return feature.Permissions.Any(p => @this.IsInRole(p.Name));
        }

        internal static string GetCleanName(this XElement @this)
        {
            return @this.Name.LocalName
                  .Replace("_STAR_", "*")
                  .Replace("_AND_", " & ")
                  .Replace("_SLASH_", "/")
                  .Replace("_DASH_", "-")
                  .Replace("_QUESTION_", "?")
                  .Replace("_", " ");
        }

        internal static string InjectId(this string @this, string id)
        {
            var urlParts = @this.Replace("{id}", id).Split('&', '?');
            return urlParts.FirstOrDefault() + urlParts.ExceptFirst().ToString("&").WithPrefix("?");
        }
    }
}