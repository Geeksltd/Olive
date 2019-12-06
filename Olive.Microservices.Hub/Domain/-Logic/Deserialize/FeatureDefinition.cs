using Olive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Domain
{
    class FeatureDefinition
    {
        XElement Data;
        Feature Feature;
        FeatureDefinition[] Children;
        FeatureDefinition Parent;
        string Name;

        public FeatureDefinition(FeatureDefinition parent, XElement data)
        {
            Parent = parent;
            Data = data;
            Name = Data.GetCleanName();
            Children = data.Elements().Select(x => new FeatureDefinition(this, x)).ToArray();
        }

        public override string ToString() => Parent?.ToString().WithSuffix(" > ") + Name;

        IEnumerable<Permission> GetPermissions(bool not = false)
        {
            var result = new List<Permission>();

            var info = Data.GetValue<string>("@permissions").OrEmpty().Split(',').Trim();

            if (Parent != null)
            {
                // Always inherit "deny" settings.
                // Inherit "grant" settings only when there is nothing explicit.
                if (not || info.None())
                {
                    result.AddRange(Parent.GetPermissions(not));
                }
            }

            foreach (var text in info)
            {
                var isNot = text.StartsWith("!");
                if (not != isNot) continue;
                var name = text.TrimStart("!");

                result.Add(Permission.GetOrCreate(name));
            }

            return result;
        }

        Service GetService()
        {
            var serviceName = Data.GetValue<string>("@service");
            if (serviceName.IsEmpty()) return Parent?.GetService();

            return Service.FindByName(serviceName)
                ?? throw new Exception("There is no service named: " + serviceName);
        }

        public IEnumerable<Feature> GetAllFeatures()
        {
            yield return GetDefinedFeature();

            foreach (var c in Children.SelectMany(x => x.GetAllFeatures()))
                yield return c;
        }

        Feature GetDefinedFeature()
        {
            if (Name.IsAnyOf("ROOT", "FOR")) return null;
            if (Feature != null) return Feature;

            var parent = Parent.GetEffectiveFeature();

            Feature = new Feature
            {
                Ref = Data.GetValue<string>("@ref"),
                Title = Name,
                Description = Data.GetValue<string>("@desc"),
                ImplementationUrl = Data.GetValue<string>("@url"),
                BadgeUrl = Data.GetValue<string>("@badgeUrl"),
                BadgeOptionalFor = Data.GetValue<string>("@badgeOptional"),
                Icon = Data.GetValue<string>("@icon"),
                PositionOrder = Data.GetValue<int?>("@order") ?? 100,
                ShowOnRight = Data.GetValue<bool?>("@showOnRight") ?? false,
                Parent = parent,
                Order = Parent.Children.IndexOf(this) * 10 + 10,
                Permissions = GetPermissions(),
                NotPermissions = GetPermissions(not: true),
                Service = GetService() ?? Service.FindByName("Hub")
            };

            Feature.UseIframe = Data.GetValue<bool?>("@iframe") ?? Feature.Service.UseIframe;

            if (Feature.ImplementationUrl.IsEmpty())
                Feature.Service = Service.FindByName("Hub");

            Feature.LoadUrl = Feature.FindLoadUrl().ToLower();

            return Feature;
        }

        Feature GetEffectiveFeature() => GetDefinedFeature() ?? Parent?.GetEffectiveFeature();
    }
}
