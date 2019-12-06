using Olive;

namespace Domain.Utilities.UrlExtensions
{
    public static class Extensions
    {
        public static string AppendUrlPath(this string @this, string path)
            => @this.WithSuffix("/" + path.TrimStart("/"));
    }
}
