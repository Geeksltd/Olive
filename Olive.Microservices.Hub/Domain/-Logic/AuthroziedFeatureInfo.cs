namespace Domain
{
    using Newtonsoft.Json;
    using Olive;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    partial class AuthroziedFeatureInfo
    {
        public static async Task<XElement> RenderMenu(Feature currentFeature)
        {
            var items = await FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User());
            return await RenderMenu(currentFeature, items);
        }

        public static async Task<XElement> RenderMenuJson()
        {
            string jsonMenu = await RenderJsonMenu();

            return new XElement("input",
                   new XAttribute("id", "topMenu"),
                   new XAttribute("type", "hidden"),
                   new XAttribute("value", jsonMenu));
        }

        static async Task<XElement> RenderMenu(Feature currentFeature,
           IEnumerable<AuthroziedFeatureInfo> items)
        {
            if (items.None()) return null;

            var rootMEnuId = Guid.NewGuid();

            if (currentFeature != null)
            {
                rootMEnuId = currentFeature.ID;
            }

            var ul = new XElement("ul", new XAttribute("class", "nav navbar-nav dropped-submenu"), new XAttribute("id", rootMEnuId));

            foreach (var item in items)
            {
                var feature = item.Feature;

                var li = new XElement("li",
                    new XAttribute("id", feature.ID),
                    new XAttribute("class", string.Format("feature-menu-item{0}", " active".OnlyWhen(feature == currentFeature)))
                    ).AddTo(ul);

                li.Add(new XAttribute("expand", (currentFeature != null && feature.WithAllChildren().Contains(currentFeature))));

                if (feature.Parent != null)
                {
                    li.Add(new XAttribute("is-side-menu-child", "true"));
                    li.Add(new XAttribute("side-menu-parent", feature.Parent.ID));
                }
                else
                {
                    li.Add(new XAttribute("is-side-menu-child", "false"));
                }

                var link = new XElement("a",
                    new XAttribute("href", feature.LoadUrl.Unless(item.IsDisabled)),
                    new XAttribute("data-badgeurl", feature.GetBadgeUrl().OrEmpty()),
                    new XAttribute("data-badge-optional", feature.IsBadgeOptional()),
                    new XAttribute("data-service", feature.Service?.Name),
                    new XAttribute("class", "badge-number"),
                    new XElement("i", string.Empty,
                        new XAttribute("class", $"{feature.Icon}"),
                        new XAttribute("aria-hidden", "true")),
                    feature.Title
                    ).AddTo(li);

                if (!item.IsDisabled && !feature.UseIframe)
                    link.Add(new XAttribute("data-redirect", "ajax"));

                var children = await FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User(), parent: feature);

                if (children.Any()) li.Add(await RenderMenu(currentFeature, children));
            }

            return ul;
        }

        public async static Task<string> RenderJsonMenu()
        {
            var items = await FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User());

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            return JsonConvert.SerializeObject(await GetAllMenuItems(items), Formatting.None, jsonSerializerSettings);
        }

        public async static Task<HashSet<JsonMenu>> GetAllMenuItems(IEnumerable<AuthroziedFeatureInfo> items)
        {
            var menuITems = new HashSet<JsonMenu>();

            foreach (var item in items)
            {
                var feature = item.Feature;

                var children = await FeatureSecurityFilter.GetAuthorizedFeatures(Context.Current.User(), parent: feature);

                var sumMenu = new JsonMenu
                {
                    ID = item.Feature.ID,
                    Icon = item.Feature.Icon,
                    Title = item.Feature.Title,
                    LoadUrl = item.Feature.LoadUrl,
                    UseIframe = item.Feature.UseIframe
                };

                if (children.Any())
                {
                    sumMenu.Children = await GetAllMenuItems(children);
                }

                menuITems.Add(sumMenu);
            }

            return menuITems;
        }
    }

    public class JsonMenu
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string LoadUrl { get; set; }
        public bool UseIframe { get; set; }
        public HashSet<JsonMenu> Children { get; set; }
    }
}