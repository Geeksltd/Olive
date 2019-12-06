using Olive;
using Olive.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Domain
{
    public static class FeatureSecurityFilter
    {
        public static async Task<IEnumerable<AuthroziedFeatureInfo>> GetAuthorizedFeatures(ClaimsPrincipal user, Feature parent = null)
        {
            var features = Feature.All.Where(f => f.Parent == parent).Except(x => x.Title == "WIDGETS");

            return await features.SelectAsync(f => GetAuthorizationInfo(user, f)).ExceptNull();
        }

        static async Task<AuthroziedFeatureInfo> GetAuthorizationInfo(ClaimsPrincipal user, Feature feature)
        {
            if (user.CanSee(feature))
                return new AuthroziedFeatureInfo { Feature = feature };

            var hasPermittedChildNodes = feature.GetAllChildren().Cast<Feature>().Any(c => user.CanSee(c));
            if (hasPermittedChildNodes)
                return new AuthroziedFeatureInfo { Feature = feature, IsDisabled = true };

            return null;
        }
    }
}