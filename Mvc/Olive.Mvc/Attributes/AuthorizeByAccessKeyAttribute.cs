using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Olive.Mvc
{
    /// <summary>
    /// A strongly typed shortcut to set [Authorize(Roles = (comma seperated access keys))].
    /// </summary>
    public class AuthorizeByAccessKeyAttribute : AuthorizeAttribute
    {
        static string[] supportedAccessKeys;

        public AuthorizeByAccessKeyAttribute()
        {
            if (supportedAccessKeys == null)
            {
                var provider = Context.Current.GetOptionalService<SourceProvider>() ?? new SettingsSourceProvider();
                supportedAccessKeys = provider.GetValidKeys();
            }

            Roles = supportedAccessKeys.OrEmpty().ToString(",");
        }

        public abstract class SourceProvider
        {
            public abstract string[] GetValidKeys();
        }

        class SettingsSourceProvider : SourceProvider
        {
            public override string[] GetValidKeys()
            {
                return Config.GetSectionOrThrow("Authentication:ApiUsers").GetChildren()
                    .SelectMany(x => x.GetChildren()).Select(x => x.Value)
                    .ToArray();
            }
        }
    }


}
